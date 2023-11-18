import React, { useEffect, useState, useRef } from 'react';
import Section from '../section';
import gamePad from '../../../icons/gamepad.svg';
import robot from '../../../icons/robot.svg';
import cart from '../../../icons/cart.svg';
import settings from '../../../icons/settings.svg';
import MenuItem from './menuItem';
import BotStats from './botStats';
import { Route, RouterProvider, Outlet, Routes } from 'react-router-dom';
import ModalDigisellerEdit from './modalDigisellerEdit';
import ModalChangePassword from './modalChangePassword';
import ModalExchangeRates from './modalExchangeRates';
import EditBotModal from '../bots/modalEdit';
import EditOrderModal from '../orders/modalEdit';
import Paggination from '../../shared/paggination';
import {
  apiSetItemActiveStatus,
  apiChangeDigisellerData,
  apiChangeUserPassword,
  apiDeleteProxyAll,
  apiEditBot,
  apiUpdateExchangeDataManual,
  apiAddGameSession,
  toggleBulkEditPercentModal,
  toggleDigisellerEditModal,
  toggleChangePasswordModal,
  toggleExchangeRatesModal,
  toggleEditBotModal,
  toggleEditOrderModal,
  toggleFilterOrdersModal,
  updateGameSessionsFilter,
  setSelectedBot,
  setStateProp,
  state,
} from '../../../containers/admin/state';
import css from './styles.scss';
import ConfirmModal from '../../shared/modalConfirm';
import gImg from '../../../icons/G.svg';
const leftMenu = () => {
  const {
    digisellerEditModalIsOpen,
    changePasswordModalIsOpen,
    exchangeRatesModalIsOpen,
    user,
    bots,
    editBotModalIsOpen,
    editOrderModalIsOpen,
    selectedBot,
    editBotResponse,
    editOrderResponse,
    selectedItems,
    exchageRates,
    gameSessionsFilter,
    gameSessionsTotal,
  } = state.use();

  const gameSessionsPages =
    gameSessionsTotal / gameSessionsFilter.size >
    (gameSessionsTotal / gameSessionsFilter.size).toFixed(0)
      ? Number((gameSessionsTotal / gameSessionsFilter.size).toFixed(0)) + 1
      : Number((gameSessionsTotal / gameSessionsFilter.size).toFixed(0));

  const [menuData, setMenuDate] = useState(menuArrData);
  const [confirmMassActiveChangeIsOpen, setConfirmMassActiveChangeIsOpen] =
    useState(false);
  return (
    <div className={css.wrapper}>
      <Section className={css.titleSection} height={65} width={254}>
        <div className={css.title}>
          {/* <span style={{ color: '#B044D6' }}>G</span> */}
          <img src={gImg} />
          <div>Pay Panel</div>
        </div>
      </Section>
      <Section className={css.menuSection} height={266} width={254}>
        <div className={css.menuList}>
          {menuData.map((i) => {
            return (
              <MenuItem
                {...i}
                isOpen={i.isOpen}
                onClick={() => {
                  if (!i.subMenu || !i.subMenu.length) return;

                  let newMenuData = [...menuData];
                  newMenuData.forEach((mi) => {
                    if (mi.name === i.name) {
                      mi.isOpen = !mi.isOpen;
                    } else {
                      mi.isOpen = false;
                    }
                  });
                  setMenuDate(newMenuData);
                }}
                onClickOutside={() => {
                  let newMenuData = [...menuData];
                  newMenuData.forEach((mi) => {
                    mi.isOpen = false;
                  });
                  setMenuDate(newMenuData);
                }}
              />
            );
          })}
        </div>
      </Section>

      <Routes>
        <Route
          path="products"
          element={
            !selectedItems || selectedItems.length === 0 ? (
              <>
                <Section
                  className={css.massButton}
                  height={49}
                  width={254}
                  onClick={() => {
                    toggleBulkEditPercentModal(true);
                  }}
                >
                  <div className={css.title}>Массовая смена цен</div>
                </Section>
                <Section
                  className={css.massButton}
                  height={49}
                  width={254}
                  onClick={() => {
                    setConfirmMassActiveChangeIsOpen(true);
                  }}
                >
                  <div className={css.title}>Массовое вкл/выкл товаров</div>
                </Section>
              </>
            ) : null
          }
        />

        <Route
          path="proxy"
          element={
            <Section
              className={css.massButton}
              height={49}
              width={254}
              onClick={() => {
                apiDeleteProxyAll();
              }}
            >
              <div className={css.title}>Удалить все прокси</div>
            </Section>
          }
        />
        <Route path="bots" element={<BotStats data={bots} />} />

        <Route
          path="orders"
          element={
            <div style={{ marginTop: '19px' }}>
              <Paggination
                val={gameSessionsFilter.page}
                max={gameSessionsPages > 0 ? gameSessionsPages : 1}
                onChange={async (page) => {
                  await updateGameSessionsFilter({ page });
                }}
              />
              <Section
                className={css.filterButton}
                height={49}
                width={254}
                onClick={() => {
                  toggleFilterOrdersModal(true);
                }}
              >
                <div className={css.title}>Фильтры</div>
              </Section>
              <Section
                className={css.filterButton}
                height={49}
                width={254}
                onClick={() => {
                  toggleEditOrderModal(true);
                }}
              >
                <div className={css.title}>Создать сессию заказа</div>
              </Section>
            </div>
          }
        />
      </Routes>

      <EditOrderModal
        isOpen={editOrderModalIsOpen}
        //value={{}}
        response={editOrderResponse}
        resetResponse={() => {
          setStateProp('editOrderResponse', {
            loading: false,
            errors: [],
          });
        }}
        onCancel={() => {
          toggleEditOrderModal(false);
        }}
        onSave={(val) => {
          apiAddGameSession(val);
        }}
      />
      <EditBotModal
        isOpen={editBotModalIsOpen}
        value={selectedBot}
        response={editBotResponse}
        resetResponse={() => {
          setStateProp('editBotResponse', {
            loading: false,
            errors: [],
          });
        }}
        onCancel={() => {
          toggleEditBotModal(false);
          setSelectedBot({});
        }}
        onSave={(val) => {
          apiEditBot(val);
        }}
      />
      <ModalDigisellerEdit
        isOpen={digisellerEditModalIsOpen}
        value={{
          digisellerApiKey: user.digisellerApiKey,
          digisellerId: user.digisellerId,
        }}
        onSave={(val) => {
          apiChangeDigisellerData(val);
          toggleDigisellerEditModal(false);
        }}
        onCancel={() => {
          toggleDigisellerEditModal(false);
        }}
      />
      <ModalChangePassword
        isOpen={changePasswordModalIsOpen}
        onSave={async (val) => {
          apiChangeUserPassword(val);
        }}
        onCancel={() => {
          toggleChangePasswordModal(false);
        }}
      />
      <ModalExchangeRates
        isOpen={exchangeRatesModalIsOpen}
        value={exchageRates}
        onSave={async (val) => {
          //console.log('newVal', val);
          apiUpdateExchangeDataManual(val);
        }}
        onCancel={() => {
          toggleExchangeRatesModal(false);
        }}
      />
      <ConfirmModal
        isOpen={confirmMassActiveChangeIsOpen}
        title={'Массовое вкл/выкл товаров'}
        content={'Вы точно хотите включить/выключить все товары?'}
        onConfirm={{
          text: 'Подтвердить',
          action: () => {
            apiSetItemActiveStatus();
          },
        }}
        onCancel={{
          action: () => {
            setConfirmMassActiveChangeIsOpen(false);
          },
        }}
      />
    </div>
  );
};

export default leftMenu;

let menuArrData = [
  {
    name: 'Товар',
    icon: gamePad,
    subMenu: [
      {
        name: 'Digiseller',
        url: '/admin/products',
      },
      {
        name: 'Магазин',
        url: '/',
      },
    ],
  },
  {
    name: 'Боты',
    icon: robot,
    subMenu: [
      {
        name: 'Список',
        url: '/admin/bots',
      },
      {
        name: 'Добавить',
        action: () => {
          toggleEditBotModal(true);
        },
      },
    ],
  },
  {
    name: 'Заказы',
    icon: cart,
    subMenu: [
      {
        name: 'Список',
        url: '/admin/orders',
      },
      {
        name: 'Создать',
        action: () => {
          toggleEditOrderModal(true);
        },
      },
    ],
    subMenuStyle: {
      top: -55,
    },
  },
  {
    name: 'Настройки',
    icon: settings,
    subMenu: [
      {
        name: 'Прокси',
        url: '/admin/proxy',
      },
      {
        name: 'Сменить пароль',
        action: () => {
          toggleChangePasswordModal(true);
        },
      },
      {
        name: 'Сменить api ключ',
        action: () => {
          toggleDigisellerEditModal(true);
        },
      },
      {
        name: 'Общие настройки',
        url: '/',
      },
      {
        name: 'Курсы валют',
        action: () => {
          toggleExchangeRatesModal(true);
        },
      },
    ],
    subMenuStyle: {
      top: -174,
      width: 198,
      right: -198,
    },
  },
];
