import React, { useState, useEffect } from 'react';
import Button from '../../../shared/button';
import ModalBase from '../../../shared/modalBase';
import Expander from './expander';
import Select from '../../../shared/select';
import css from './styles.scss';
import { state } from '../../../../containers/admin/state';
import moment from 'moment';
import { Outlet, Link, useNavigate, useLocation } from 'react-router-dom';

const ModalStatusHistory = ({ isOpen, data, onCancel }) => {
    let hist = data?.statusHistory;
    const { gameSessionsStatuses: statuses } = state.use();
    const navigate = useNavigate();

    const createBotLink = (val) => {
        return (
            <span
                className={css.link}
                onClick={() => {
                    navigate('/admin/bots?id=' + val.botId);
                    if (onCancel) onCancel();
                }}
            >
                {val.botName}
            </span>
        );
    };

    const createUserLink = (val) => {
        return (
            <span
                className={css.link}
                onClick={() => {
                    window.open(val.userProfileUrl, '_blank');
                }}
            >
                {val.userNickname}
            </span>
        );
    };

    return (
    <ModalBase
      isOpen={isOpen}
      title={`Лог заказа #${data?.id}`}
      width={582}
      height={783}
    >
      <div className={css.content}>
        {hist &&
          Object.keys(hist)?.map((gr) => {
            let grTitle = moment(gr).format('DD.MM.YYYY');
            return (
              <div className={css.logGroup}>
                <div className={css.title}>{grTitle}</div>
                {hist[gr].map((log) => {
                  //console.log(log);
                  let logDate = moment(log.insertDate).format(
                    'DD.MM.YYYY HH:mm:ss'
                  );
                  let status = statuses[log.statusId];

                  const renderContent = (log, status) => {
                    let val = log.value;
                    let sid = log.statusId;
                    if (sid === 12 && val) {
                      return (
                        <div>
                          {!val.message.includes('Сброс') &&
                            !val.message.includes('Смена') && (
                              <div>{val.message}</div>
                            )}
                          {val.oldUserProfileUrl && (
                            <div>
                              Предыдущее значение: {val.oldUserProfileUrl}
                            </div>
                          )}
                        </div>
                      );
                    } else if (sid === 18 && val) {
                      return <div>Отправляем игру с {createBotLink(val)}</div>;
                    } else if (sid === 3 && val) {
                      return (
                        <div>
                          <div>{status.description}</div>
                          {val.message && <div>{val.message}</div>}
                          {val.userProfileUrl && (
                            <div>Значение: {val.userProfileUrl}</div>
                          )}
                        </div>
                      );
                    } else if (sid === 7 && val) {
                      return (
                        <div>
                          <div>{val.message}</div>
                          {val.userSteamContact && (
                            <div>
                              Указанный контакт для добавления:{' '}
                              {val.userSteamContact}
                            </div>
                          )}
                          {val.userProfileUrl && (
                            <div>
                              Полученный профиль пользователя:{' '}
                              {val.userProfileUrl}
                            </div>
                          )}
                          {val.botName && val.botId && (
                            <div>Бот: {createBotLink(val)}</div>
                          )}
                        </div>
                      );
                    } else if (sid === 16 && val) {
                      return (
                        <div>
                          <div>{val.message}</div>
                          <div>
                            Указанный контакт для добавления:{' '}
                            {val.userSteamContact}
                          </div>
                          {val.userProfileUrl && (
                            <div>
                              Полученный профиль пользователя:{' '}
                              {val.userProfileUrl}
                            </div>
                          )}
                        </div>
                      );
                    } else if (sid === 2 && val) {
                      return (
                        <div>
                          {`Получателю с никнеймом `} {createUserLink(val)}
                          {` была отправлена игра с бота `}
                          {createBotLink(val)}
                        </div>
                      );
                    } else if (sid === 17 && val && val.botFilter) {
                      return (
                        <div>
                          <div>Критерии фильтрации:</div>
                          <div>регион - {val.botFilter.selectedRegion}</div>
                          <div>
                            особенность бота -{' '}
                            {val.botFilter.withMaxBalance
                              ? 'с большим балансом'
                              : 'меньше всего попыток отправки игр по лимиту за час'}
                          </div>
                        </div>
                      );
                    } else if (sid === 4 && val) {
                      return (
                        <div>
                          <div>
                            Получатель отклонил заявку от бота{' '}
                            {createBotLink(val)}
                          </div>
                        </div>
                      );
                    } else if (sid === 6 && val) {
                      return (
                        <div>
                          <div>
                            {`Получателю с никнеймом `} {createUserLink(val)}
                            {` была отправлена заявка в друзья с бота `}
                            {createBotLink(val)}
                          </div>
                          <div>
                            Страна бота:{' '}
                            <span style={{ color: '#d836e7' }}>
                              {val.botRegionName || val.botRegionCode}
                            </span>
                          </div>
                        </div>
                      );
                    }
                    else if (val)
                    {
                        return (
                            <><div>{status.description}</div>
                                <div>
                                <div>{val.message}</div>
                                {val.userSteamContact && (
                                    <div>
                                        Указанный контакт для добавления:{' '}
                                        {val.userSteamContact}
                                    </div>
                                )}
                                {val.userProfileUrl && (
                                    <div>
                                        Полученный профиль пользователя:{' '}
                                        {val.userProfileUrl}
                                    </div>
                                )}
                                {val.userNickname && (
                                    <div>
                                        Никнейм:{' '}
                                        {val.userNickname}
                                    </div>
                                )}
                                {val.itemPrice && (
                                    <div>
                                        Стоимость товара:{' '}
                                        {val.itemPrice}
                                    </div>
                                )}
                                {val.itemRegion && (
                                    <div>
                                        Регион товара:{' '}
                                        {val.itemRegion}
                                    </div>
                                )}
                                {val.botName && val.botId && (
                                    <div>Бот: {createBotLink(val)}</div>
                                )}
                                {val.botRegionName && val.botRegionCode && (
                                    <div>
                                        Страна бота:{' '}
                                        <span style={{ color: '#d836e7' }}>
                                            {val.botRegionName || val.botRegionCode}
                                        </span>
                                    </div>
                                )}
                                {val.botFilter && (
                                    <div>
                                        <div>Критерии фильтрации:</div>
                                        <div>регион - {val.botFilter.selectedRegion}</div>
                                        <div>
                                            особенность бота -{' '}
                                            {val.botFilter.withMaxBalance
                                                ? 'с большим балансом'
                                                : 'меньше всего попыток отправки игр по лимиту за час'}
                                        </div>
                                    </div>
                                )}
                            </div></>
                        )
                    }
                    else {
                      return <div>{status.description}</div>;
                    }
                  };

                  let headerName = status.name;
                  let headerColor = status.color;
                  if (log.statusId === 12) {
                    let mes = (log.value && log.value.message) || '';
                    if (mes.includes('Сброс') || mes.includes('Смена')) {
                      headerName = mes;
                      headerColor = '#DDE11C';
                    }
                  }

                  return (
                    <div className={css.expanderWrapper}>
                      <Expander
                        header={
                          <div style={{ fontSize: '14px', display: 'flex' }}>
                            <div style={{ marginRight: '20px' }}>{logDate}</div>
                            <div style={{ color: headerColor }}>
                              {headerName}
                            </div>
                          </div>
                        }
                        content={
                          <div
                            style={{ fontSize: '12px', wordWrap: 'break-word' }}
                          >
                            {renderContent(log, status)}
                          </div>
                        }
                      />
                    </div>
                  );
                })}
              </div>
            );
          })}
      </div>

      <div className={css.actions}>
        <Button
          text={'Закрыть'}
          onClick={async () => {
            if (onCancel) onCancel();
          }}
          style={{ backgroundColor: '#9A7AA9', marginLeft: '0px' }}
        />
      </div>
    </ModalBase >
  );
};

export default ModalStatusHistory;
