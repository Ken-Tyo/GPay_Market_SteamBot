import React from 'react';
import css from './styles.scss';
import botavatar from '../../../../icons/botavatar.svg';
import addbot from '../../../../icons/addbot.svg';
import vacReturn from '../../../../icons/vacReturn.svg';
import warning from '../../../../icons/warning2.svg';
import BotDetailsModal from '../modalBotDetails';
import RemoveBotModal from '../../../shared/modalConfirm';
import Switch from '../../../shared/switch';
import { stateColor, currenciesSymbol, getFlagByRegionCode } from '../utils';
import {
  state,
  toggleEditBotModal,
  apiDeleteBot,
  apiBotSetIsOn,
  apiSaveBotRegionSettings,
  setSelectedBot,
  toggleEditBotRegionSetModal,
  toggleBotDetailsModal,
} from '../../../../containers/admin/state';
import Popover from '@mui/material/Popover';
import RegionSettingsEdit from '../modalRegionSettingsEdit';
import { styled } from '@mui/material/styles';
import Button from '@mui/material/Button';
import Tooltip, { tooltipClasses } from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
const grid = () => {
  const {
    bots,
    selectedBot,
    currencies,
    saveBotRegionSetResponse,
    botRegionSetEditModalIsOpen,
    botDetailsModalIsOpen,
  } = state.use();
  const [remModalIsOpen, setRemModalIsOpen] = React.useState(false);
  const [showVac, setShowVac] = React.useState({});

  let currencyDict = {};
  currencies.map((c) => {
    currencyDict[c.steamId] = c;
  });

  return (
    <>
      <div className={css.grid}>
        {bots.map((i) => {
          let imgSrc = i.avatarUrl || botavatar;

          let state = stateColor[1];
          if (i.state) {
            state = stateColor[i.state];
          }

          let vacGames = i.vacGames && i.vacGames.filter((vg) => vg.hasVac);

          if (showVac[i.id]) {
            return (
              <div
                className={css.item}
                style={{ borderLeft: `3px solid ${state}` }}
              >
                <div className={css.vacSide}>
                  <div className={css.list}>
                    {vacGames.map((vg) => {
                      return <div className={css.vacItem}>{vg.name}</div>;
                    })}
                  </div>
                  <div className={css.vacBackButWrapper}>
                    <img
                      src={vacReturn}
                      className={css.vacBackBut}
                      onClick={() => {
                        let newShowVac = { ...showVac };
                        delete newShowVac[i.id];
                        setShowVac(newShowVac);
                      }}
                    />
                  </div>
                </div>
              </div>
            );
          }

          const isRegionProblem = i.isProblemRegion && !i.botRegionSetting;

          return (
            <div
              className={css.item}
              style={{ borderLeft: `3px solid ${state}` }}
            >
              <div className={css.header}>
                <div className={css.image}>
                  <img
                    src={imgSrc}
                    onClick={() => {
                      if (i.steamId)
                        window.open(
                          `https://steamcommunity.com/profiles/${i.steamId}`,
                          '_blank'
                        );
                    }}
                  />
                  {isRegionProblem && (
                    <div
                      className={css.problem}
                      onClick={() => {
                        setSelectedBot(i);
                        toggleEditBotRegionSetModal(true);
                      }}
                    >
                      <HtmlTooltip
                        title={
                          <React.Fragment>
                            <div>
                              <div
                                style={{
                                  fontSize: '16px',
                                  lineHeight: '20px',
                                }}
                              >
                                Проблемный регион:{' '}
                                <span style={{ color: '#B93ED8' }}>Китай</span>{' '}
                                !
                              </div>
                              <div
                                style={{
                                  fontSize: '14px',
                                  lineHeight: '20px',
                                  marginTop: '5px',
                                }}
                              >
                                БОТ отключен от работы, поскольку обнаружен
                                проблемный регион или валюта при сборе суммы
                                покупок. Настройте аккаунт для возобновления
                                работы с данным ботов
                              </div>
                            </div>
                          </React.Fragment>
                        }
                      >
                        <img src={warning} />
                      </HtmlTooltip>
                    </div>
                  )}
                </div>
                <div className={css.info}>
                  <div
                    className={css.login}
                    onClick={() => {
                      setSelectedBot(i);
                      toggleBotDetailsModal(true);
                    }}
                  >
                    {i.userName}
                  </div>
                  <div className={css.data}>
                    <div className={css.region}>{i.region} </div>
                    <div className={css.flag}>
                      <img src={getFlagByRegionCode(i.region)} height={13} />
                    </div>
                    <div
                      className={css.proxy}
                      onClick={() => {
                        setSelectedBot(i);
                        toggleBotDetailsModal(true);
                      }}
                    >
                      {i.proxyStr}
                    </div>
                  </div>
                </div>
              </div>

              {vacGames && vacGames.length > 0 ? (
                <div className={css.vacWrapper}>
                  <div
                    className={css.vac}
                    onClick={() => {
                      let newShowVac = { ...showVac };
                      newShowVac = Object.assign(newShowVac, { [i.id]: true });

                      setShowVac(newShowVac);
                    }}
                  >
                    <div className={css.text}>{vacGames.length} VAC</div>
                  </div>
                  <div className={css.lineWrapper}>
                    <div className={css.line}></div>
                  </div>
                </div>
              ) : (
                <div className={css.vacWrapper} style={{}}>
                  <div className={css.lineWrapper}>
                    <div
                      className={css.line}
                      style={{
                        width: '240px',
                        marginLeft: '62px',
                        marginTop: '18px',
                        marginBottom: '12px',
                      }}
                    ></div>
                  </div>
                </div>
              )}

              <div className={css.prices}>
                <div className={css.left}>
                  <div className={css.text}>
                    {i.balance} {currencyDict[i.steamCurrencyId]?.steamSymbol}
                  </div>
                </div>
                  {(i.remainingSumToGift === null || i.remainingSumToGift === undefined)
                    ?
                    <div className={css.right}>
                      <div className={css.leftPrice}>
                        <div>{i.totalPurchaseSumUSD.toFixed(2)} $</div>
                      </div>
                      <div className={css.rightPrice}>
                        <div style={{}}>
                          <span style={{ color: '#C39F1C' }}>
                            {i.sendedGiftsSum.toFixed(2)}{' '}
                            {currencyDict[i.steamCurrencyId]?.steamSymbol}
                          </span>{' '}
                          / {i.maxSendedGiftsSum.toFixed(2)}{' '}
                          {currencyDict[i.steamCurrencyId]?.steamSymbol}
                        </div>
                      </div>
                    </div>
                    :
                    <LimitContainer bot={i} currDict={currencyDict} />
                }
              </div>

              <div className={css.buttonWrapper}>
                <div className={css.isOnBut}>
                  <Switch
                    value={i.isON}
                    onChange={(val) => apiBotSetIsOn(i.id, val)}
                    style={{ transform: 'scale(0.96)' }}
                    lastSaveTime={i.lastTimeUpdated}
                  />
                </div>
                <div className={css.buttons}>
                  <div
                    className={css.editBtn}
                    onClick={() => {
                      if (isRegionProblem) return;
                      setSelectedBot(i);
                      toggleEditBotModal(true);
                    }}
                  >
                    Редактировать
                  </div>
                  <div
                    className={css.delBtn}
                    onClick={() => {
                      setSelectedBot(i);
                      setRemModalIsOpen(true);
                    }}
                  >
                    Удалить
                  </div>
                </div>
              </div>
            </div>
          );
        })}
        <div
          className={css.addItem}
          onClick={() => {
            setSelectedBot({});
            toggleEditBotModal(true);
          }}
        >
          <img src={addbot} />
        </div>
      </div>

      <RegionSettingsEdit
        isOpen={botRegionSetEditModalIsOpen}
        value={{}}
        data={selectedBot}
        response={saveBotRegionSetResponse}
        onSave={(val) => {
          let req = { ...val, botId: selectedBot.id };
          console.log(req);
          apiSaveBotRegionSettings(req);
        }}
        onCancel={() => {
          toggleEditBotRegionSetModal(false);
        }}
      />

      <BotDetailsModal
        isOpen={botDetailsModalIsOpen}
        data={selectedBot}
        onCancel={() => {
          toggleBotDetailsModal(false);
        }}
      />
      <RemoveBotModal
        title="Подтвердите удаление"
        isOpen={remModalIsOpen}
        content={`Вы действительно хотите удалить бота ${selectedBot.userName} ?`}
        onConfirm={{
          action: () => {
            apiDeleteBot(selectedBot.id);
            setRemModalIsOpen(false);
          },
        }}
        onCancel={{
          action: () => {
            setRemModalIsOpen(false);
          },
        }}
      />
    </>
  );
};
const LimitContainer = ({ bot, currDict }) => {
    return (
        <div className={css.remainingRight}>
            <div className={css.remainingPrice}>
                <div style={{}}>
                    {'до '}
                    <span style={{ color: '#C5443C' }}>
                        {(bot.remainingSumToGift * currDict[bot.steamCurrencyId]?.steamValue).toFixed(2)}
                    </span>
                    {' '}{currDict[bot.steamCurrencyId]?.steamSymbol}
                </div>
            </div>
        </div>
    );
} 
const HtmlTooltip = styled(({ className, ...props }) => (
  <Tooltip {...props} classes={{ popper: className }} />
))(({ theme }) => ({
  [`& .${tooltipClasses.tooltip}`]: {
    backgroundColor: '#43294B',
    color: '#D4D4D4',
    width: 389,
    padding: '13px 20px',
    fontSize: theme.typography.pxToRem(12),
    borderRadius: '16px',
  },
}));

export default grid;
