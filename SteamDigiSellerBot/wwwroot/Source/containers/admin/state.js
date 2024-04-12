import { entity } from 'simpler-state';
import { mapToFormData, getUrlQueryParams } from '../../utils/common';
import { itemsMode as iMode } from '../../containers/admin/common';
const signalR = require('@microsoft/signalr');

export const state = entity({
  user: { digisellerId: '', digisellerApiKey: '' },
  activeMenuLink: '',
  items: [],
  gameSessions: [],
  gameSessionsTotal: 0,
  gameSessionsFilter: {
    appId: '',
    gameName: '',
    orderId: null,
    profileStr: '',
    uniqueCodes: '',
    steamCurrencyId: null,
    statusId: null,
    page: 1,
    size: 50,
  },
  gameSessionsStatuses: {},
  bots: [],
  proxies: [],
  selectedBot: {},
  selectedItem: {},
  selectedItems: [],
  exchageRates: [],
  currencies: [],
  steamRegions: [],
  digiPriceSetType: [
    { id: 1, name: '%' },
    { id: 2, name: '₽' },
  ],
  editBotResponse: { loading: false, errors: [] },
  editOrderResponse: { loading: false, errors: [] },
  saveBotRegionSetResponse: { loading: false, errors: [] },
  itemsMode: iMode[1],
  itemsLoading: false,
  bulkEditPercentModalIsOpen: false,
  digisellerEditModalIsOpen: false,
  changePasswordModalIsOpen: false,
  loadProxiesModalIsOpen: false,
  addGameSesCommentIsOpen: false,
  editBotModalIsOpen: false,
  exchangeRatesModalIsOpen: false,
  editItemModalIsOpen: false,
  editOrderModalIsOpen: false,
  filterOrdersModalIsOpen: false,
  botRegionSetEditModalIsOpen: false,
  statusHistoryModalIsOpen: false,
  botDetailsModalIsOpen: false,
  wsconn: null,
});

export const initAdmin = async () => {
  const { wsconn } = state.get();

  if (!wsconn) {
    let connection = new signalR.HubConnectionBuilder()
      //.configureLogging(signalR.LogLevel.Trace)
      .withUrl('/adminhub')
      .build();
    setStateProp('wsconn', connection);
    connection.on('Notify', async function (mes) {
      //let arg = data.arguments[0];
      console.log(mes);
      if (!mes) return;

      switch (mes.type) {
        case 1: {
          let gs = await apiFetchGameSession(mes.data.gsId);
          if (gs) {
            state.set((value) => {
              let gsList = value.gameSessions;
              let idx = gsList.map((gs) => gs.id).indexOf(gs.id);
              gsList[idx] = gs;

              return {
                ...value,
                gameSessions: gsList,
              };
            });
          }
        }
      }
    });

    await connection.start();
  }

  // .then(() => {})
  // .catch((err) => {
  //   console.log(err);
  // });
};

export const initBotsPage = async () => {
  await apiFetchBots();
  await apiGetCurrencies();

  let params = getUrlQueryParams();
  console.log('params', params);
  let botId = Number(params.id || 0);
  if (botId) {
    const { bots } = state.get();
    let bot = bots.find((b) => b.id === botId);
    //console.log(botId, bots, bot);
    if (bot) {
      await setSelectedBot(bot);
      toggleBotDetailsModal(true);
    }
  }
};

export const setStateProp = (name, val) => {
  state.set((value) => {
    return {
      ...value,
      [name]: val,
    };
  });
};

export const setItems = (items) => {
  state.set((value) => {
    return {
      ...value,
      items: items,
    };
  });
};

export const setBots = (items) => {
  state.set((value) => {
    return {
      ...value,
      bots: items,
    };
  });
};

export const setProxies = (items) => {
  state.set((value) => {
    return {
      ...value,
      proxies: items,
    };
  });
};

export const setSelectedBot = async (bot) => {
  state.set((value) => {
    return {
      ...value,
      selectedBot: bot,
    };
  });
};

export const setSelectedItem = async (item) => {
  state.set((value) => {
    return {
      ...value,
      selectedItem: item,
    };
  });
};

export const setSelectedItems = (bot) => {
  state.set((value) => {
    return {
      ...value,
      selectedItems: bot,
    };
  });
};

export const setActiveMenuLink = (name) => {
  state.set((value) => {
    return {
      ...value,
      activeMenuLink: name,
    };
  });
};

export const apiFetchItems = async () => {
  let res = await fetch('/items/list');
  setItems(await res.json());
};

export const setItemPrice = async (gpId, newPrice, itemId) => {
  let res = await apiSetItemPrice(gpId, newPrice);
  if (res.ok) {
    let item = await apiGetItem(itemId);
    setSelectedItem(item);
  }
};
export const apiSetItemPrice = async (gpId, newPrice) => {
  let res = await fetch(`/items/price/${gpId}/${newPrice}`, { method: 'POST' });
  return res;
};

export const setItemPricePriority = async (gpId, itemId) => {
  let res = await apiSetItemPricePriority(gpId);
  if (res.ok) {
    let item = await apiGetItem(itemId);
    setSelectedItem(item);
  }
};

export const apiSetItemPricePriority = async (gpId) => {
  let res = await fetch(`/items/price/${gpId}/priority`, { method: 'POST' });
  console.log(res);
  return res;
};

export const apiFetchGameSessions = async (filter) => {
  let res = await fetch('/gamesessions/list', {
    method: 'POST',
    body: mapToFormData({
      appId: filter.appId,
      gameName: filter.gameName,
      orderId: filter.orderId,
      profileStr: filter.profileStr,
      uniqueCodes: filter.uniqueCodes,
      steamCurrencyId: filter.steamCurrencyId,
      page: filter.page,
      statusId: filter.statusId,
    }),
  });

  const json = await res.json();
  setStateProp('gameSessions', json.list);
  setStateProp('gameSessionsTotal', json.total);
};

export const apiFetchGameSessionsWithCurrentFilter = async () => {
  let filter = state.get().gameSessionsFilter;
  await apiFetchGameSessions(filter);
};

export const apiFetchGameSession = async (gsId) => {
  let res = await fetch(`/gamesessions/${gsId}`);

  if (res.ok) {
    const json = await res.json();
    return json;
  }
  return null;
};

export const apiFetchGameSessStatuses = async () => {
  let gameSessionsStatuses = state.get().gameSessionsStatuses;
  if (gameSessionsStatuses && Object.keys(gameSessionsStatuses).length > 0)
    return;

  let res = await fetch('/gamesessions/statuses');
  setStateProp('gameSessionsStatuses', await res.json());
};

export const apiFetchBots = async () => {
  let res = await fetch('/bots/list');
  setBots(await res.json());
};

export const apiFetchProxies = async () => {
  let res = await fetch('/proxy/list');
  setProxies(await res.json());
};

export const apiSetItemActiveStatus = async (ids) => {
  let idParam = '';
  if (ids && ids.length > 0) {
    idParam = ids.join(',');
  }

  if (!ids || ids.length > 1) setItemsLoading(true);

  let res = await fetch(`/items/SetActive?ids=${idParam}`);
  await apiFetchItems();
  if (!ids || ids.length > 1) setItemsLoading(false);
};

export const apiDeleteItem = async (id) => {
  let res = await fetch(`/items/Delete?id=${id}`);
  if (res.ok) {
    state.set((value) => {
      return {
        ...value,
        items: value.items.filter((i) => i.id !== id),
      };
    });
  }
};

export const apiBulkDeleteItem = async (Ids) => {
  let res = await fetch(`/items/bulk/delete`, {
    method: 'POST',
    body: mapToFormData({
      Ids,
    }),
  });

  await apiFetchItems();
  return res.ok;
};

export const apiDeleteProxy = async (id) => {
  let res = await fetch(`/proxy/delete?id=${id}`);
  if (res.ok) {
    state.set((value) => {
      return {
        ...value,
        proxies: value.proxies.filter((i) => i.id !== id),
      };
    });
  }
};

export const apiDeleteProxyAll = async (id) => {
  let res = await fetch(`/proxy/delete/all`);
  if (res.ok) {
    state.set((value) => {
      return {
        ...value,
        proxies: [],
      };
    });
  }
};

export const apiLoadNewProxy = async (data) => {
  let res = await fetch(`/proxy/load`, {
    method: 'POST',
    body: mapToFormData(data),
  });
  if (!res.ok) {
    return;
  }
  toggleLoadProxiesModal(false);
  await apiFetchProxies();
};

export const apiEditBot = async (item) => {
  setStateProp('editBotResponse', {
    loading: true,
    errors: [],
  });

  let res = await fetch(`/bots/add`, {
    method: 'POST',
    body: mapToFormData(item),
  });

  let errors = [];
  if (res.ok) {
    toggleEditBotModal(false);
    await apiFetchBots();
  } else {
    if (res.status === 500) {
      errors.push('Произошла непредвиденная ошибка, проверьте консоль.');
    } else {
      errors = (await res.json()).errors;
    }
  }

  setStateProp('editBotResponse', {
    loading: false,
    errors: errors,
  });

  return res.ok;
};

export const apiDeleteBot = async (id) => {
  let res = await fetch(`/bots/Delete?id=${id}`);
  if (res.ok) {
    state.set((value) => {
      return {
        ...value,
        bots: value.bots.filter((i) => i.id !== id),
      };
    });
  }
};

export const apiBotSetIsOn = async (id, isOn) => {
  let res = await fetch(`/bots/setison`, {
    method: 'POST',
    body: mapToFormData({
      botId: id,
      isOn: isOn,
    }),
  });

  //await apiFetchBots();
};

export const apiSaveBotRegionSettings = async (item) => {
  setStateProp('saveBotRegionSetResponse', {
    loading: true,
    errors: [],
  });

  let res = await fetch(`/bots/regionsettings`, {
    method: 'POST',
    body: mapToFormData(item),
  });

  let errors = [];
  if (res.ok) {
    toggleEditBotRegionSetModal(false);
    await apiFetchBots();
  } else {
    if (res.status === 500) {
      errors.push('Произошла непредвиденная ошибка, проверьте консоль.');
    } else {
      errors = (await res.json()).errors;
    }
  }

  setStateProp('saveBotRegionSetResponse', {
    loading: false,
    errors: errors,
  });

  return res.ok;
};

export const apiChangeItem = async (item) => {
  setItemsLoading(true);
  toggleEditItemModal(false);

  let res = await fetch(`/items/edit/${item.id}`, {
    method: 'POST',
    body: mapToFormData(item),
  });

  if (res.ok) {
    await apiFetchItems();
  } else {
    toggleEditItemModal(true);
  }
  setItemsLoading(false);
};

export const apiCreateItem = async (item) => {
  setItemsLoading(true);
  toggleEditItemModal(false);

  let res = await fetch(`/items/add`, {
    method: 'POST',
    body: mapToFormData(item),
  });

  if (res.ok) {
    await apiFetchItems();
  } else {
    toggleEditItemModal(true);
  }
  setItemsLoading(false);
};

export const setItemsLoading = (isOn) => {
  state.set((value) => {
    return {
      ...value,
      itemsLoading: isOn,
    };
  });
};

export const toggleBulkEditPercentModal = (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      bulkEditPercentModalIsOpen: isOpen,
    };
  });
};

export const toggleLoadProxiesModal = (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      loadProxiesModalIsOpen: isOpen,
    };
  });
};

export const toggleAddGameSesCommentModal = (isOpen) => {
  setStateProp('addGameSesCommentIsOpen', isOpen);
};

export const toggleViewStatusHistoryModal = (isOpen) => {
  setStateProp('statusHistoryModalIsOpen', isOpen);
};

export const toggleDigisellerEditModal = async (isOpen) => {
  if (isOpen === true) {
    await apiGetCurrentUser();
  }

  state.set((value) => {
    return {
      ...value,
      digisellerEditModalIsOpen: isOpen,
    };
  });
};

export const toggleExchangeRatesModal = async (isOpen) => {
  if (isOpen === true) {
    await apiGetExchageRates();
  }

  state.set((value) => {
    return {
      ...value,
      exchangeRatesModalIsOpen: isOpen,
    };
  });
};

export const toggleBotDetailsModal = (isOpen) => {
  setStateProp('botDetailsModalIsOpen', isOpen);
};

export const toggleChangePasswordModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      changePasswordModalIsOpen: isOpen,
    };
  });
};

export const toggleEditBotModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      editBotModalIsOpen: isOpen,
    };
  });
};

export const toggleEditBotRegionSetModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      botRegionSetEditModalIsOpen: isOpen,
    };
  });
};

export const toggleEditOrderModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      editOrderModalIsOpen: isOpen,
    };
  });
};

export const toggleFilterOrdersModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      filterOrdersModalIsOpen: isOpen,
    };
  });
};

export const toggleEditItemModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      editItemModalIsOpen: isOpen,
    };
  });
};

export const toggleOrderCreationInfoModal = async(isOpen) => {
    state.set((value) => {
        return {
            ...value,
            orderCreationInfoIsOpen: isOpen,
        };
    });
};
  setItemsLoading(true);
  let res = await fetch(`/items/bulk/change`, {
    method: 'POST',
    body: mapToFormData({
      SteamPercent,
      Ids,
    }),
  });

  await apiFetchItems();
  setItemsLoading(false);
};

export const apiChangeDigisellerData = async (data) => {
  let res = await fetch(`/user/edit/digiseller`, {
    method: 'POST',
    body: mapToFormData(data),
  });
};

export const apiChangeUserPassword = async (data) => {
  let res = await fetch(`/user/password`, {
    method: 'POST',
    body: mapToFormData(data),
  });

  toggleChangePasswordModal(!res.ok);
};

export const apiGetCurrentUser = async () => {
  let res = await fetch(`/user`);
  let data = await res.json();
  state.set((value) => {
    return {
      ...value,
      user: data,
    };
  });
};

export const apiGetExchageRates = async () => {
  let res = await fetch(`/exchangerates/list`);
  let data = await res.json();
  state.set((value) => {
    return {
      ...value,
      exchageRates: data,
    };
  });
};

export const apiUpdateExchangeDataManual = async (data) => {
  //let res = await
  fetch(`/exchangerates/update`, {
    method: 'POST',
    body: mapToFormData(data),
  });

  //toggleExchangeRatesModal(!res.ok);
  toggleExchangeRatesModal(false);
};

export const apiGetCurrencies = async () => {
  let currencies = state.get().currencies;
  if (currencies && currencies.length > 0) return;

  let res = await fetch(`/exchangerates/list`);
  let data = await res.json();

  currencies = data.currencies.map((c) => {
    return {
      code: c.code,
      steamId: c.steamId,
      steamSymbol: c.steamSymbol,
    };
  });

  state.set((value) => {
    return {
      ...value,
      currencies: currencies,
    };
  });
};

export const apiGetItem = async (id) => {
  let res = await fetch(`/items/${id}/info`);
  let data = await res.json();

  return data;
};

export const apiGetSteamRegions = async () => {
  let steamRegions = state.get().steamRegions;
  if (steamRegions && steamRegions.length > 0) return;

  let res = await fetch(`/dict/regions`);
  let data = await res.json();

  steamRegions = data;
  setStateProp('steamRegions', steamRegions);
};

export const apiSetGameSessionStatus = async (gSesId, statusId) => {
  const { gameSessionsFilter } = state.get();

  let res = await fetch(`/gamesessions/setstatus`, {
    method: 'POST',
    body: mapToFormData({
      gameSessionId: gSesId,
      statusId: statusId,
    }),
  });

  apiFetchGameSessions(gameSessionsFilter);
};

export const apiResetGameSession = async (gSesId) => {
  const { gameSessionsFilter } = state.get();

  let res = await fetch(`/gamesessions/reset`, {
    method: 'POST',
    body: mapToFormData({
      gameSessionId: gSesId,
    }),
  });

  apiFetchGameSessions(gameSessionsFilter);
};

export const apiAddCommentGameSession = async (gSesId, comment) => {
  const { gameSessionsFilter } = state.get();
  toggleAddGameSesCommentModal(false);

  let res = await fetch(`/gamesessions/comment`, {
    method: 'POST',
    body: mapToFormData({
      gameSessionId: gSesId,
      comment: comment,
    }),
  });

  apiFetchGameSessions(gameSessionsFilter);
};

export const updateGameSessionsFilter = async (newData) => {
  const { gameSessionsFilter } = state.get();
  let newFilter = {
    ...gameSessionsFilter,
    ...newData,
  };
  console.log('new f', newFilter);

  setStateProp('gameSessionsFilter', newFilter);
  await apiFetchGameSessions(newFilter);
};

export const apiAddGameSession = async (data) => {
  setStateProp('editOrderResponse', {
    loading: true,
    errors: [],
  });

  let res = await fetch(`/gamesession`, {
    method: 'POST',
    body: mapToFormData(data),
  });

  let errors = [];
  if (res.ok) {
    const { gameSessionsFilter } = state.get();
    await apiFetchGameSessions(gameSessionsFilter);
    toggleEditOrderModal(false);
  } else {
    if (res.status === 500) {
      errors.push('Произошла непредвиденная ошибка, проверьте консоль.');
    } else {
      errors = (await res.json()).errors;
    }
  }

  setStateProp('editOrderResponse', {
    loading: false,
    errors: errors,
  });

    if (res.ok) {
        var newUniqueCodes = await res.json();
        console.log(newUniqueCodes);
        state.set((value) => {
            return {
                ...value,
                newUniqueCodes: newUniqueCodes,
            };
        });
        toggleOrderCreationInfoModal(true);
    }
};
