import React, { useEffect, useState } from 'react';
import moment from 'moment';
import Section from '../../section';
import CircularProgress from '@mui/material/CircularProgress';
import IconButton from '../../../shared/iconButton';
import Button from '../../../shared/button';
import List from '../../../shared/list';
import AddCommentModal from '../../../shared/modalSaveText';
import css from './styles.scss';
import StatusBadge from './statusBadge';
import referArrow from '../../../../icons/referArrow.svg';
import comment from '../../../../icons/comment.svg';
import check from '../../../../icons/check.svg';
import retry from '../../../../icons/retry.svg';
import stop from '../../../../icons/stop.svg';
import {
  state,
  apiSetGameSessionStatus,
  apiResetGameSession,
  apiAddCommentGameSession,
  toggleAddGameSesCommentModal,
  toggleViewStatusHistoryModal,
} from '../../../../containers/admin/state';
import ConfirmDialog from '../../../shared/modalConfirm';
import { useLocation } from 'react-router-dom';
import StatusHistory from '../modalStatusHistory';
const products = () => {
  const {
    gameSessions,
    currencies,
    addGameSesCommentIsOpen,
    statusHistoryModalIsOpen,
  } = state.use();

  const [openConfirm, setOpenConfirm] = useState(false);
  const [editItem, setEditItem] = useState(null);
  const [confirmData, setConfirmData] = useState({});

  const currencyDict = {};
  currencies.map((c) => {
    currencyDict[c.steamId] = c;
  });

  let location = useLocation();
  const { host, origin } = window.location;

  return (
    <div className={css.wrapper}>
      <List
        data={[...gameSessions]}
        headers={Object.values(headers)}
        itemRenderer={(i) => {
          return (
            <tr>
              <td>
                <div className={css.cell}>
                  <div className={css.id}>#{i.id}</div>
                </div>
              </td>
              <td>
                <div className={css.cell}>
                  <div className={css.uniqueId}>
                    <div>{i.uniqueCode}</div>
                    <div className={css.ref}>
                      <a
                        href={`${origin}?uniquecode=${i.uniqueCode}`}
                        target="_blank"
                      >
                        <img src={referArrow} />
                      </a>
                    </div>
                  </div>
                </div>
              </td>
              <td>
                <div className={css.cell}>{i.region}</div>
              </td>
              <td>
                <div
                  className={css.cell}
                  style={{ justifyContent: 'start', paddingLeft: '20px' }}
                >
                  <div className={css.info}>
                    <div>{i.gameName}</div>
                    <div className={css.secondRow}>
                      <div>
                        {moment(i.addedDateTime).format(
                          'DD.MM.YYYY | HH:mm:ss'
                        )}
                      </div>
                      {i.steamProfileUrl && (
                        <div className={css.profileRef}>
                          <a target="_blank" href={i.steamProfileUrl}>
                            {'<профиль>'}
                          </a>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              </td>
              <td>
                <div className={css.cell}>
                  <div className={css.comment}>
                    <img
                      src={comment}
                      style={{
                        filter: i.comment
                          ? 'brightness(200%)'
                          : 'brightness(100%)',
                      }}
                      onClick={() => {
                        setEditItem(i);
                        toggleAddGameSesCommentModal(true);
                      }}
                    />
                  </div>
                </div>
              </td>
              <td>
                <div className={css.cell}>
                  {i.itemPrice ? `${i.itemPrice.toFixed(2)} ₽` : '-'}{' '}
                  {i.itemSteamPercent ? `(${i.itemSteamPercent}%)` : ''}
                </div>
              </td>
              <td>
                <div className={css.cell}>{i.botName ? i.botName : '-'}</div>
              </td>
              <td>
                <div className={css.cell}>
                  <StatusBadge
                    data={i.status}
                    onClick={() => {
                      setEditItem(i);
                      toggleViewStatusHistoryModal(true);
                    }}
                  />
                </div>
              </td>
              <td>
                <div className={css.cell}>
                  <div className={css.buttons}>
                    <div className={css.btnWrapper}>
                      <IconButton
                        icon={check}
                        onClick={() => {
                          let data = getConfirmTypeData(3);
                          data.onConfirm.action = () => {
                            setOpenConfirm(false);
                            apiSetGameSessionStatus(i.id, 1);
                          };
                          setConfirmData(data);
                          setOpenConfirm(true);
                        }}
                        disabled={i.status.statusId === 1}
                      />
                    </div>
                    <div className={css.btnWrapper}>
                      <IconButton
                        icon={retry}
                        onClick={() => {
                          let data = getConfirmTypeData(2);
                          data.onConfirm.action = () => {
                            setOpenConfirm(false);
                            apiResetGameSession(i.id);
                          };
                          setConfirmData(data);
                          setOpenConfirm(true);
                        }}
                      />
                    </div>
                    <div className={css.btnWrapper}>
                      <IconButton
                        icon={stop}
                        onClick={() => {
                          let data = getConfirmTypeData(1);
                          data.onConfirm.action = () => {
                            setOpenConfirm(false);
                            apiSetGameSessionStatus(i.id, 15);
                          };
                          setConfirmData(data);
                          setOpenConfirm(true);
                        }}
                        disabled={i.status.statusId === 15}
                      />
                    </div>
                  </div>
                </div>
              </td>
              <td>
                <div className={css.cell}></div>
              </td>
            </tr>
          );
        }}
      />

      <AddCommentModal
        title={'Комментарий к заказу'}
        defaultValue={editItem?.comment}
        placeholder={
          'введите комментарий к заказу. комментарий будет виден только вам'
        }
        isOpen={addGameSesCommentIsOpen}
        onSave={{
          label: 'Сохранить',
          action: (val) => {
            apiAddCommentGameSession(editItem?.id, val);
          },
        }}
        onCancel={{
          label: 'Отмена',
          action: () => {
            toggleAddGameSesCommentModal(false);
          },
        }}
        height={551}
      />

      <ConfirmDialog
        title={confirmData.title}
        content={
          <div style={{ width: '501px', textAlign: 'center' }}>
            {confirmData.subtitle}
          </div>
        }
        isOpen={openConfirm}
        onConfirm={confirmData.onConfirm}
        height={confirmData.height}
        onCancel={{
          action: () => {
            setOpenConfirm(false);
          },
        }}
      />

      <StatusHistory
        isOpen={statusHistoryModalIsOpen}
        data={editItem}
        onCancel={() => {
          toggleViewStatusHistoryModal(false);
        }}
      />
    </div>
  );
};

export default products;

const getConfirmTypeData = (type) => {
  let res = {};
  switch (type) {
    case 1:
      res = {
        title: 'Подтвердите закрытие заказа',
        subtitle:
          'Вы действительно хотите закрыть сессию заказа? Открыть ее будет невозможно',
        onConfirm: {
          text: 'Закрыть',
        },
        height: 220,
      };
      break;
    case 2:
      res = {
        title: 'Подтвердите отката заказа',
        subtitle:
          'Вы действительно хотите откатить сессию заказа до изначального состояния?',
        onConfirm: {
          text: 'Сбросить',
        },
        height: 220,
      };
      break;
    case 3:
      res = {
        title: 'Подтвердите выполнение заказа',
        subtitle: 'Вы действительно хотите подтвердить выполнение заказа?',
        onConfirm: {
          text: 'Подтвердить',
          bg: '#478C35',
        },
        height: 220,
      };
      break;
  }

  return res;
};

const headers = {
  id: 'ID',
  orderId: 'Уникальный код',
  region: '',
  info: 'Информация',
  comment: '',
  price: 'Цена',
  botName: 'Бот',
  status: 'Статус',
  options: 'Опции',
};
