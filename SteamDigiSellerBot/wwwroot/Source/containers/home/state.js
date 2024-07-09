import { entity } from 'simpler-state';
import {
  mapToFormData,
  getUrlQueryParams,
  promisify,
} from '../../utils/common';
import { useSearchParams } from 'react-router-dom';
import moment from 'moment';
const signalR = require('@microsoft/signalr');

let initState = {
  showCaptcha: false,
  lastOrders: [],
  isCorrectCode: true,
  isParamExits: true,
  gameSession: {},
  checkCodeErr: '',
  checkCodeLoading: false,
  autoInvitationTimeOut: null,
  wsconn: null,
};

export const state = entity(initState);

export const init = async () => {
  await startWebSockets();
  await Promise.all([
    promisify(apiFetchLastOrders),
    promisify(checkCodeWithParams),
  ]);
};

const startWebSockets = async () => {
  const { wsconn } = state.get();
  if (wsconn) return;

  let connection = new signalR.HubConnectionBuilder()
    //.configureLogging(signalR.LogLevel.Trace)
    .withUrl('/homehub')
    .build();

  const start = async () => {
    try {
      await connection.start();
      console.log('SignalR Connected.');
      setStateProp('wsconn', connection);
    } catch (err) {
      console.log(err);
      setTimeout(start, 5000);
    }
  };

  connection.onclose(async () => {
    await start();
  });

  connection.on('Notify', async function (mes) {
    //let arg = data.arguments[0];
    console.log(mes);
    if (!mes) return;

    switch (mes.type) {
      case 1: {
        const { gameSession, isCorrectCode } = state.get();
        if (isCorrectCode && gameSession.uniqueCode === mes.data.uniqueCode) {
          let res = await apiCheckCode(gameSession.uniqueCode, '', '');
          const respData = await res.json();

          setStateProp('checkCodeErr', respData.errorCode);
          setStateProp('showCaptcha', respData.isRobotCheck);
          setStateProp('isCorrectCode', respData.isCorrectCode);
          setStateProp('gameSession', respData.gameSession);
          setStateProp('checkCodeLoading', false);

          let gs = respData.gameSession;
          if (gs.statusId === 1 || gs.statusId === 2) {
            let url = `https://digiseller.market/info/buy.asp?id_i=${gs.digisellerId}&lang=ru-RU`;
            window.open(url, '_blank');
          }
        }
      }
    }
  });

  // Start the connection.
  await start();
};

export const setStateProp = (name, val) => {
  state.set((value) => {
    return {
      ...value,
      [name]: val,
    };
  });
};

export const checkCode = async (uniquecode, seller_id, captcha) => {
  if (uniquecode) setStateProp('checkCodeLoading', true);
  await checkCodeCommon(uniquecode, seller_id, captcha);
  setStateProp('checkCodeLoading', false);
};

export const checkCodeWithParams = async () => {
  const params = getUrlQueryParams();
  const uniquecode = params['uniquecode'] || '';
  const seller_id = params['seller_id'] || '';
  const captcha = params['captcha'] || '';

  checkCodeCommon(uniquecode, seller_id, captcha);
};

const checkCodeCommon = async (uniquecode, seller_id, captcha) => {
  const isParamExits = !!uniquecode;
  setStateProp('isParamExits', isParamExits);

  const res = await apiCheckCode(uniquecode, seller_id, captcha);

  const respData = await res.json();
  //console.log('respData', respData);
  setStateProp('checkCodeErr', respData.errorCode);
  setStateProp('showCaptcha', respData.isRobotCheck);
  setStateProp('isCorrectCode', respData.isCorrectCode);
  setStateProp('gameSession', respData.gameSession);

  const { wsconn } = state.get();

  if (respData.isCorrectCode) await wsconn.invoke('SendUniqueCode', uniquecode);
};

export const apiCheckCode = async (uniquecode, seller_id, captcha) => {
  setStateProp('checkCodeLoadingModal', true);
  let res = await fetch(`/home/checkCode`, {
    method: 'POST',
    body: mapToFormData({
      uniquecode: uniquecode,
      seller_id: seller_id,
      captcha: captcha,
    }),
  });

  setStateProp('checkCodeLoadingModal', false);
  //console.log(res);
  return res;
};

export const apiFetchLastOrders = async () => {
  let res = await fetch('/home/lastOrders');
  setStateProp('lastOrders', await res.json());
};

export const apiSetSteamContact = async (uniquecode, steamContact) => {
  setStateProp('checkCodeLoading', true);

  let res = await fetch(`/gamesession/steamcontact`, {
    method: 'POST',
    body: mapToFormData({
      uniquecode: uniquecode,
      steamContact: steamContact,
    }),
  });

  if (res.ok) {
    let gameSession = await res.json();
    setStateProp('gameSession', gameSession);

    let leftSec = moment.duration(
      moment(gameSession.autoSendInvitationTime).diff(moment())
    );

    // let timeoutMs = (leftSec.seconds() + 2) * 1000;
    // let autoInvitation = setTimeout(async () => {
    //   await apiCheckCode(gameSession.uniqueCode, '', '');
    //   console.log('auto initation sended');
    // }, timeoutMs);

    // setStateProp('autoInvitationTimeOut', autoInvitation);
  }
  setStateProp('checkCodeLoading', false);
};

export const apiResetSteamAcc = async () => {
  const { gameSession, autoInvitationTimeOut } = state.get();
  //clearTimeout(autoInvitationTimeOut);
  //setStateProp('autoInvitationTimeOut', null);

  let res = await fetch(`/gamesession/resetsteamacc`, {
    method: 'POST',
    body: mapToFormData({
      uniquecode: gameSession.uniqueCode,
    }),
  });

  if (res.ok) {
    let gameSession = await res.json();
    setStateProp('gameSession', gameSession);
  }
};

// export const resetProfileUrl = () => {
//   const { gameSession } = state.get();

//   gameSession.statusId = 12;
//   gameSession.steamProfileUrl = null;

//   setStateProp('gameSession', gameSession);
// };

export const apiConfirmSending = async (uniquecode) => {
  setStateProp('checkCodeLoading', true);

  let res = await fetch(`/gamesession/confirmsending`, {
    method: 'POST',
    body: mapToFormData({
      uniquecode: uniquecode,
    }),
  });

  if (res.ok) {
    let gameSession = await res.json();
    setStateProp('gameSession', gameSession);
  }
  //setStateProp('checkCodeLoading', false);
};

export const apiCheckFriend = async (uniquecode) => {
  setStateProp('checkCodeLoading', true);

  let res = await fetch(`/gamesession/checkfrined`, {
    method: 'POST',
    body: mapToFormData({
      uniquecode: uniquecode,
    }),
  });
};
