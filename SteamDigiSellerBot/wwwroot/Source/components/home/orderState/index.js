import React, { useEffect, useState } from 'react';
import css from './styles.scss';
import {
    state,
    apiSetSteamContact,
    apiConfirmSending,
    apiCheckFriend,
    checkCode,
    apiResetSteamAcc,
    apiResetBot
} from '../../../containers/home/state';
import { useSearchParams, useNavigate } from 'react-router-dom';
import moment from 'moment';
import info from '../../../icons/info.svg';
import close from './close.svg';
import Tooltip, { tooltipClasses } from '@mui/material/Tooltip';
import { styled } from '@mui/material/styles';
import ReCAPTCHA from 'react-google-recaptcha';
import { useTranslation } from 'react-i18next';
import CircularLoader from '../../shared/circularLoader';
import Button from '../button';

const OrderState = () => {
    const {
        isParamExits,
        isCorrectCode,
        gameSession,
        showCaptcha,
        checkCodeErr,
        checkCodeLoading,
        checkCodeLoadingModal,
    } = state.use();
    //console.log('state', state.use());
    const { t: tCheckCode, i18n } = useTranslation('checkCode');
    const { t: tOrderState } = useTranslation('orderState');
    const { t: tActTimeExpires } = useTranslation('activationTimeExpires');
    const { t: tInvitationRefused } = useTranslation('invitationRefused');
    const { t: tInvitationRefusedWithRemoteBot } = useTranslation('invitationRefusedWithRemoteBot');
    const { t: tGameExists } = useTranslation('gameExists');
    const { t: tGameSended } = useTranslation('gameSended');
    const { t: tGameSendInProgress } = useTranslation('gameSendInProgress');
    const { t: tGameInQueue } = useTranslation('gameInQueue');
    const { t: tRegionError } = useTranslation('regionError');
    const { t: tGameRequiredError } = useTranslation('gameRequired');
    const { t: tError } = useTranslation('error');
    const { t: tOrderClosed } = useTranslation('orderClosed');
    const { t: tCommon } = useTranslation('common');
    const { t: tTempInviteBan } = useTranslation('tempInviteBan');

    const navigate = useNavigate();
    const recaptchaRef = React.createRef();

    const isDlc = gameSession?.isDlc;
    const isIncorrectProfileUrl = gameSession && gameSession.statusId === 3;
    const discountEndDate = gameSession?.sessionEndTime;

    const showEnterUniqueCode = !isParamExits || !isCorrectCode;
    //let showActivationTimeExpires = moment(new Date()) > moment(discountEndDate);

    const showActivationTimeExpires =
        gameSession && (gameSession.statusId === 11 || gameSession.statusId === 10);
    const showEnterProfileUrl =
        gameSession && (gameSession.statusId === 3 || gameSession.statusId === 12);
    const showConfirmProfileUrl =
        gameSession && gameSession.statusId === 16 && gameSession.steamProfileUrl;
    const showIvitationSended = gameSession && gameSession.statusId === 6;
    const showInvitationRefused = gameSession && gameSession.statusId === 4 && gameSession.botName;
    const showTempInviteBan = gameSession && gameSession.statusId === 22 && gameSession.botName;
    const showInvitationRefusedWithRemoteBot = gameSession && gameSession.statusId === 4 && !gameSession.botName;
    const showGameAlreadyExists = gameSession && gameSession.statusId === 14;
    const showSendInProgress = gameSession && gameSession.statusId === 18;
    const showInQueue = gameSession && gameSession.statusId === 19;
    const showGameSended =
        gameSession && (gameSession.statusId === 1 || gameSession.statusId === 2);
    const showRegionError = gameSession && gameSession.statusId === 5;
    const showGameRequiredError = gameSession && gameSession.statusId === 23;
    const showOrderClosed = gameSession && gameSession.statusId === 15;
    const showError =
        gameSession && (gameSession.statusId === 17 || gameSession.statusId === 7);
    const showWaiting =
        gameSession && (gameSession.statusId === 20 || gameSession.statusId === 21);

    let [searchParams, setSearchParams] = useSearchParams();
    const uniquecode = searchParams.get('uniquecode') || '';

    const [isResendBlocked, setIsResendBlocked] = useState(true);

    useEffect(() => {
        // Запускаем таймер на 60 секунд
        const timer = setTimeout(() => {
            setIsResendBlocked(false); // Разблокируем кнопку через 60 секунд
        }, 60000);

        // Очищаем таймер при размонтировании компонента
        return () => clearTimeout(timer);
    }, []);



    return (
        <div className={css.wrapper}>
            {checkCodeLoadingModal && uniquecode !== '' &&
                <Area
                    title='Происходит подгрузка заказа...'
                >
                    <div className={css.enterProfileUrl}><CircularLoader color={'#571676'} /></div></Area>}
            {showEnterUniqueCode && (
                // <Area title={'Введите уникальный код заказа'} width={823}>
                <Area title={tCheckCode('title')}>
                    <div className={css.enterUniqueCode}>
                        {!checkCodeLoading && (
                            <>
                                <div className={css.form}>
                                    <InputWithButton
                                        defaultValue={uniquecode}
                                        butName={tCheckCode('inputAcceptBut')}
                                        placeholder={tCheckCode('inputPlaceholder')}
                                        onClick={async (val) => {
                                            const recaptchaValue = recaptchaRef?.current?.getValue();
                                            recaptchaRef?.current?.reset();
                                            checkCode(val ?? '', '', recaptchaValue ?? '');
                                        }}
                                    />

                                    {showCaptcha && (
                                        <div className={css.captha}>
                                            <ReCAPTCHA
                                                ref={recaptchaRef}
                                                sitekey="6Lel764kAAAAAIhSZK3QvwBRgLeYrMKds7FdgCa9"
                                                hl={i18n.language}
                                            />
                                        </div>
                                    )}
                                </div>

                                {checkCodeErr && checkCodeErr > 1 && (
                                    <div style={{ marginTop: '20px' }} className={css.errorText}>
                                        {tCheckCode(`errors.${checkCodeErr}`)}
                                    </div>
                                )}
                            </>
                        )}

                        {checkCodeLoading && <CircularLoader color={'#571676'} />}
                    </div>
                </Area>
            )}

            {showEnterProfileUrl && (
                <Area
                    title={
                        <div
                            style={{
                                display: 'flex',
                                flexDirection: 'column',
                                alignItems: 'center',
                            }}
                        >
                            <div>
                                {tOrderState('title')} #{gameSession.id}
                            </div>
                            <div className={css.enterProfileUrlOrderName}>
                                {gameSession.itemName}
                            </div>
                        </div>
                    }
                >
                    <div className={css.enterProfileUrl}>
                        {!checkCodeLoading && (
                            <>
                                <div className={css.hints}>
                                    <div className={css.hint}>{tOrderState('steamUrlInfo1')}</div>
                                    <div className={css.hint}>{tOrderState('steamUrlInfo2')}</div>
                                </div>
                                <InputWithButton
                                    butName={tOrderState('inputAcceptBut')}
                                    placeholder={tOrderState('inputPlaceholder')}
                                    onClick={(val) => {
                                        if (val) apiSetSteamContact(gameSession.uniqueCode, val);
                                    }}
                                />
                                {isIncorrectProfileUrl && (
                                    <div style={{ marginTop: '20px' }} className={css.errorText}>
                                        {tOrderState('linkIncorrectErr')}
                                    </div>
                                )}

                                <Dlc isDlc={isDlc} />
                                <Timer endTime={discountEndDate} />
                            </>
                        )}

                        {checkCodeLoading && <CircularLoader color={'#571676'} />}
                    </div>
                </Area>
            )}

            {showConfirmProfileUrl && (
                <Area
                    title={
                        <div
                            style={{
                                display: 'flex',
                                flexDirection: 'column',
                                alignItems: 'center',
                            }}
                        >
                            <div>
                                {tOrderState('title')} #{gameSession.id}
                            </div>
                            <div className={css.showConfirmProfileUrlName}>
                                {gameSession.itemName}
                            </div>
                        </div>
                    }
                >
                    <div className={css.confirmProfileUrl}>
                        {!checkCodeLoading && (
                            <>
                                <div className={css.accImg}>
                                    <img src={gameSession.steamProfileAvatarUrl} />
                                </div>

                                <div className={css.hints}>
                                    <div className={css.hint}>
                                        {tOrderState('yourSteamName')}:{' '}
                                        <span style={{ color: '#8615BC' }}>
                                            {gameSession.steamProfileName}.
                                        </span>
                                        <br />
                                        {gameSession.steamProfileUrl}
                                    </div>
                                </div>

                                <div className={css.accButtons}>
                                    <Button
                                        text={tOrderState('thisMyAccBut')}
                                        style={{ backgroundColor: '#571676' }}
                                        onClick={() => {
                                            apiConfirmSending(gameSession.uniqueCode);
                                        }}
                                    />
                                    {!gameSession.blockOrder && (
                                        <Button
                                            text={tOrderState('changeAccountBut')}
                                            style={{
                                                backgroundColor: '#FFFFFF',
                                                color: '#8615BC',
                                                border: '1px solid #571676',
                                                marginRight: '1.5em'
                                            }}
                                            onClick={() => {
                                                apiResetSteamAcc();
                                            }}
                                        />)}
                                </div>

                                <Dlc isDlc={isDlc} />
                                <Timer endTime={discountEndDate} />
                            </>
                        )}
                        {checkCodeLoading && <CircularLoader color={'#571676'} />}
                    </div>
                </Area>
            )}

            {showIvitationSended && (
                <Area title={tOrderState('requestSent')}>
                    <div className={css.ivitationSended}>
                        {!checkCodeLoading && (
                            <>
                                <div className={css.accImg}>
                                    <img src={gameSession.steamProfileAvatarUrl} />
                                </div>

                                <div className={css.hints}>
                                    <div className={css.hint}>
                                        {tOrderState('yourSteamName')}:{' '}
                                        <span style={{ color: '#8615BC' }}>
                                            {gameSession.steamProfileName}.
                                        </span>
                                        <br />
                                        {gameSession.steamProfileUrl}
                                    </div>
                                    <div
                                        className={css.hint}
                                        dangerouslySetInnerHTML={{
                                            __html: tOrderState('requestSentInfo', {
                                                botName: `<a href="${gameSession.botProfileUrl}" 
                                     style="color: #8615BC; text-decoration: none; " 
                                     target="_blank" >${gameSession.botName}</a>`,
                                            }),
                                        }}
                                    ></div>
                                </div>

                                <div className={css.accButtons}>
                                    {!gameSession.blockOrder && (
                                        <Button
                                            text={tOrderState('changeAccountBut')}
                                            style={{
                                                backgroundColor: '#FFFFFF',
                                                color: '#8615BC',
                                                border: '1px solid #571676',
                                                marginRight: '1.5em'
                                            }}
                                            onClick={() => {
                                                apiResetSteamAcc();
                                            }}
                                        />)}
                                    <div style={{ display: 'flex', alignItems: 'center' }}>
                                        <ContactTheSeller digisellerId={gameSession.digisellerId} />
                                    </div>
                                </div>

                                <Dlc isDlc={isDlc} />
                                <Timer endTime={discountEndDate} />
                            </>
                        )}
                        {checkCodeLoading && <CircularLoader color={'#571676'} />}
                    </div>
                </Area>
            )}

            {showWaiting && (
                <Area title="">
                    <div className={css.ivitationSended}>
                        <div className={css.accImg}>
                            <img src={gameSession.steamProfileAvatarUrl} />
                        </div>

                        <div className={css.hints}>
                            <div className={css.hint}>
                                {tOrderState('yourSteamName')}:{' '}
                                <span style={{ color: '#8615BC' }}>
                                    {gameSession.steamProfileName}.
                                </span>
                                <br />
                                {gameSession.steamProfileUrl}
                            </div>
                            <div
                                className={css.hint}
                                dangerouslySetInnerHTML={{
                                    __html: tOrderState('requestInProcessing'),
                                }}
                            ></div>
                        </div>
                        <div style={{ marginTop: '1.5rem', marginBottom: '3rem' }} >
                            <CircularLoader color={'#571676'} />
                        </div>
                        <div className={css.accButtons}>
                            {!gameSession.blockOrder && (
                                <Button
                                    text={tOrderState('changeAccountBut')}
                                    style={{
                                        backgroundColor: '#FFFFFF',
                                        color: '#8615BC',
                                        border: '1px solid #571676',
                                        marginRight: '1.5em'
                                    }}
                                    onClick={() => {
                                        apiResetSteamAcc();
                                    }}
                                />)}
                            <div style={{ display: 'flex', alignItems: 'center' }}>
                                <ContactTheSeller digisellerId={gameSession.digisellerId} />
                            </div>
                        </div>

                    </div>
                </Area>
            )}

            {showInvitationRefused && (
                <Area
                    title={`${tCommon('order')} #${gameSession.id} - ${tInvitationRefused(
                        'error'
                    )}`}
                >
                    <div className={css.ivitationRefused}>
                        {!checkCodeLoading && (
                            <>
                                <div className={css.hints}>
                                    <div className={css.hint}>
                                        {tInvitationRefused('youRefused')}
                                    </div>
                                    <div
                                        className={css.hint}
                                        dangerouslySetInnerHTML={{
                                            __html: tInvitationRefused('info', {
                                                botName: `<a href="${gameSession.botInvitationUrl ||
                                                    gameSession.botProfileUrl
                                                    }" 
                                     style="color: #8615BC; text-decoration: none; " 
                                     target="_blank" >${gameSession.botName
                                                    }</a>`,
                                            }),
                                        }}
                                    ></div>
                                </div>

                                <div className={css.accButtons}>
                                    <div className={css.line1}>
                                        {!gameSession.blockOrder && (
                                            <Button
                                                text={tOrderState('changeAccountBut')}
                                                style={{
                                                    backgroundColor: '#FFFFFF',
                                                    color: '#8615BC',
                                                    border: '1px solid #571676',
                                                    marginRight: '1.5em'
                                                }}
                                                onClick={() => {
                                                    apiResetSteamAcc();
                                                }}
                                            />)}
                                        <Button
                                            text={tInvitationRefused('iSentInvitation')}
                                            className={css.but}
                                            onClick={() => {
                                                apiCheckFriend(gameSession.uniqueCode);
                                            }}
                                        />
                                    </div>
                                    {gameSession.isAnotherBotExists && (
                                        <div className={css.line1} style={{ marginTop: '32px' }}>
                                            <Button
                                                text={tInvitationRefused('tryWithBot')}
                                                className={css.but}
                                                style={{ width: '377px' }}
                                                onClick={() => {
                                                    apiResetBot();
                                                }}
                                            />
                                        </div>
                                    )}

                                    <div className={css.contactSellerWrapper}>
                                        <ContactTheSeller digisellerId={gameSession.digisellerId} />
                                    </div>
                                </div>

                                <Dlc isDlc={isDlc} />
                                <Timer endTime={discountEndDate} />
                            </>
                        )}
                        {checkCodeLoading && <CircularLoader color={'#571676'} />}
                    </div>
                </Area>
            )}

            {showInvitationRefusedWithRemoteBot && (
                <Area
                    title={`${tCommon('order')} #${gameSession.id} - ${tInvitationRefused(
                        'error'
                    )}`}
                >
                    <div className={css.ivitationRefused}>
                        {!checkCodeLoading && (
                            <>
                                <div className={css.hints}>
                                    <div className={css.hint}>
                                        {tInvitationRefused('youRefused')}
                                    </div>
                                    <div
                                        className={css.hint}
                                        dangerouslySetInnerHTML={{
                                            __html: tInvitationRefusedWithRemoteBot('info'),
                                        }}
                                    ></div>
                                </div>

                                <div className={css.accButtons}>
                                    <div className={css.line1}>
                                        {!gameSession.blockOrder && (
                                            <Button
                                                text={tOrderState('changeAccountBut')}
                                                style={{
                                                    backgroundColor: '#FFFFFF',
                                                    color: '#8615BC',
                                                    border: '1px solid #571676',
                                                    marginRight: '1.5em'
                                                }}
                                                onClick={() => {
                                                    apiResetSteamAcc();
                                                }}
                                            />)}
                                    </div>
                                    {gameSession.isAnotherBotExists && (
                                        <div className={css.line1} style={{ marginTop: '32px' }}>
                                            <Button
                                                text={tInvitationRefused('tryWithBot')}
                                                className={css.but}
                                                style={{ width: '377px' }}
                                                onClick={() => {
                                                    apiResetBot();
                                                }}
                                            />
                                        </div>
                                    )}

                                    <div className={css.contactSellerWrapper}>
                                        <ContactTheSeller digisellerId={gameSession.digisellerId} />
                                    </div>
                                </div>

                                <Dlc isDlc={isDlc} />
                                <Timer endTime={discountEndDate} />
                            </>
                        )}
                        {checkCodeLoading && <CircularLoader color={'#571676'} />}
                    </div>
                </Area>
            )}

            {showGameAlreadyExists && (
                <Area
                    title={`${tCommon('order')} #${gameSession.id} - ${tGameExists(
                        'error'
                    )}`}
                >
                    <div className={css.gameAlreadyExists}>
                        {!checkCodeLoading && (
                            <>
                                <div className={css.hints}>
                                    <div
                                        className={css.hint}
                                        dangerouslySetInnerHTML={{
                                            __html: tGameExists('info', {
                                                link: `<a href="https://store.steampowered.com/account/licenses/" 
                                     style="color: #8615BC; text-decoration: none; " 
                                     target="_blank" >
                                      ${tGameExists('hereClick')}
                                     </a>`,
                                            }),
                                        }}
                                    ></div>
                                </div>

                                <div className={css.accButtons}>
                                    <div className={css.line1}>
                                        {!gameSession.blockOrder && (
                                            <Button
                                                text={tOrderState('changeAccountBut')}
                                                style={{
                                                    backgroundColor: '#FFFFFF',
                                                    color: '#8615BC',
                                                    border: '1px solid #571676',
                                                    marginRight: '1.5em'
                                                }}
                                                onClick={() => {
                                                    apiResetSteamAcc();
                                                }}
                                            />)}
                                        {gameSession.canResendGame && (
                                            <Button
                                                text={tGameExists('repeatSendGame')}
                                                className={css.but}
                                                style={{ marginLeft: '25px' }}
                                                onClick={() => {
                                                    apiCheckFriend(gameSession.uniqueCode);
                                                }}
                                            />
                                        )}
                                    </div>

                                    <div
                                        className={css.contactSellerWrapper}
                                        style={{ marginTop: '20px' }}
                                    >
                                        <ContactTheSeller digisellerId={gameSession.digisellerId} />
                                    </div>
                                </div>

                                <Dlc isDlc={isDlc} />
                                <Timer endTime={discountEndDate} />
                            </>
                        )}
                        {checkCodeLoading && <CircularLoader color={'#571676'} />}
                    </div>
                </Area>
            )}

            {showGameSended && (
                <Area
                    title={<div style={{ color: '#18C82A' }}>{tGameSended('title')}</div>}
                    onClose={async () => {
                        navigate('/');
                        window.location = window.location.pathname;
                    }}
                >
                    <div className={css.gameSended}>
                        {!checkCodeLoading && (
                            <>
                                <div className={css.hints}>
                                    <div className={css.hint}>{tGameSended('info')}</div>
                                </div>

                                <div className={css.accButtons}>
                                    <div className={css.line1}>
                                        <Button
                                            text={tGameSended('homeBut')}
                                            className={css.but}
                                            style={{ marginRight: '25px' }}
                                            onClick={() => {
                                                window.location = window.location.pathname;
                                            }}
                                        />
                                        <Button
                                            text={tGameSended('feedbackBut')}
                                            className={css.leaveFeedback}
                                            onClick={() => {
                                                let url = `https://digiseller.market/info/buy.asp?id_i=${gameSession.digisellerId}&lang=ru-RU`;
                                                window.open(url, '_blank');
                                            }}
                                        />
                                    </div>
                                </div>

                                <CheckEditionVersion />
                            </>
                        )}
                        {checkCodeLoading && <CircularLoader color={'#571676'} />}
                    </div>
                </Area>
            )}

            {showSendInProgress && (
                <Area
                    title={`${tCommon('order')} #${gameSession.id
                        } - ${tGameSendInProgress('title')}`}
                >
                    <div className={css.sendInProgress}>
                        <>
                            <div className={css.hints}>
                                <div className={css.hint}>
                                    <div className={css.loader}>
                                        <CircularLoader
                                            color={'#571676'}
                                            height={137}
                                            width={137}
                                        />
                                    </div>
                                    <div className={css.loaderMob}>
                                        <CircularLoader color={'#571676'} height={76} width={76} />
                                    </div>
                                </div>
                                <div className={css.hint}>
                                    {tGameSendInProgress('info1')} <br />
                                    {tGameSendInProgress('info2')}
                                </div>
                            </div>

                            <div className={css.accButtons}>
                                <ContactTheSeller digisellerId={gameSession.digisellerId} />
                            </div>
                        </>
                    </div>
                </Area>
            )}

            {showInQueue && (
                <Area
                    title={
                        <div>
                            {tCommon('order')} #{gameSession.id} - {tGameInQueue('queue')}{' '}
                            <span style={{ color: '#8615BC' }}>
                                № {gameSession.queuePosition}
                            </span>
                        </div>
                    }
                >
                    <div className={css.gameInQueue}>
                        <>
                            <div className={css.hints}>
                                <div className={css.hint}>
                                    {tGameInQueue('info1')} {gameSession.queuePosition}.{' '}
                                    {tGameInQueue('info2')} {gameSession.queueWaitingMinutes} min.
                                </div>
                                <div className={css.hint}>{tGameInQueue('info3')}</div>
                            </div>

                            <div className={css.accButtons}>
                                <div className={css.line1}>
                                    {!gameSession.blockOrder && (
                                        <Button
                                            text={tOrderState('changeAccountBut')}
                                            style={{
                                                backgroundColor: '#FFFFFF',
                                                color: '#8615BC',
                                                border: '1px solid #571676',
                                                marginRight: '1.5em'
                                            }}
                                            onClick={() => {
                                                apiResetSteamAcc();
                                            }}
                                        />)}

                                    <div className={css.contactSellerWrapper}>
                                        <ContactTheSeller digisellerId={gameSession.digisellerId} />
                                    </div>
                                </div>
                            </div>

                            <Dlc isDlc={isDlc} />
                            <Timer endTime={discountEndDate} />
                        </>
                    </div>
                </Area>
            )}

            {showError && (
                <Area
                    title={`${tCommon('order')} #${gameSession.id} - ${tInvitationRefused(
                        'error'
                    )}`}
                >
                    <div className={css.error}>
                        {!checkCodeLoading && (
                            <>
                                <div className={css.hints}>
                                    <div className={css.hint}>{tError('info')}</div>
                                </div>

                                <div className={css.accButtons}>
                                    <div className={css.line1}>
                                        {!gameSession.blockOrder && (
                                            <Button
                                                text={tOrderState('changeAccountBut')}
                                                style={{
                                                    backgroundColor: '#FFFFFF',
                                                    color: '#8615BC',
                                                    border: '1px solid #571676',
                                                    marginRight: '1.5em'
                                                }}
                                                onClick={() => {
                                                    apiResetSteamAcc();
                                                }}
                                            />)}

                                        <div
                                            style={{
                                                display: 'flex',
                                                alignItems: 'center',
                                            }}
                                        >
                                            <ContactTheSeller
                                                digisellerId={gameSession.digisellerId}
                                            />
                                        </div>
                                    </div>
                                </div>
                            </>
                        )}
                        {checkCodeLoading && <CircularLoader color={'#571676'} />}
                    </div>
                </Area>
            )}

            {showActivationTimeExpires && (
                <Area>
                    <div className={css.activationExpErapper}>
                        <div className={css.title}>{tActTimeExpires('error')}</div>
                        <div className={css.text}>{tActTimeExpires('message')}</div>
                        <ContactTheSeller digisellerId={gameSession.digisellerId} />
                    </div>
                </Area>
            )}

            {showRegionError && (
                <Area>
                    <div className={css.regionError}>
                        <div className={css.title}>{tRegionError('error')}</div>
                        <div className={css.text}>{tRegionError('info')}</div>
                        <div className={css.accButtons}>
                            <div className={css.line1}>
                                {!gameSession.blockOrder && (
                                    <Button
                                        text={tOrderState('changeAccountBut')}
                                        style={{
                                            backgroundColor: '#FFFFFF',
                                            color: '#8615BC',
                                            border: '1px solid #571676',
                                            marginRight: '1.5em'
                                        }}
                                        onClick={() => {
                                            apiResetSteamAcc();
                                        }}
                                    />)}

                                <div className={css.contactBut}>
                                    <ContactTheSeller digisellerId={gameSession.digisellerId} />
                                </div>
                            </div>
                        </div>
                    </div>
                </Area>
            )}

            {showGameRequiredError && (
                <Area>
                    <div className={css.regionError}>
                        <div className={css.title}>{tGameRequiredError('error')}</div>
                        <div className={css.text} style="margin-bottom:2rem">{tGameRequiredError('info_row1')}</div>
                        <div className={css.text} style="margin-bottom:1.5rem">{tGameRequiredError('info_row2')}</div>
                        <div className={css.text} style="margin-bottom:1rem">{tGameRequiredError('info_row3')}</div>
                        <div className={css.text}>{tGameRequiredError('info_row4')}</div>
                        <div className={css.accButtons}>
                            <div className={css.line1}>
                                {(!gameSession.blockOrder && gameSession.canResendGame) && (
                                    <Button
                                        text={tGameRequiredError('repeatSendGame')}
                                        className={css.but}
                                        style={
                                            {
                                                marginRight: '1.5em',
                                                opacity: isResendBlocked ? 0.7 : 1,
                                                pointerEvents: isResendBlocked ? 'none' : 'auto',
                                            }}
                                        onClick={() => {
                                            setIsResendBlocked(true);
                                            apiCheckFriend(gameSession.uniqueCode);
                                        }}
                                        disabled={isResendBlocked}
                                    />
                                )}
                                {!gameSession.blockOrder && (
                                    <Button
                                        text={tOrderState('changeAccountBut')}
                                        style={{
                                            backgroundColor: '#FFFFFF',
                                            color: '#8615BC',
                                            border: '1px solid #571676',

                                            marginLeft: '25px'
                                        }}
                                        onClick={() => {
                                            apiResetSteamAcc();
                                        }}
                                    />)}
                            </div>

                            <div
                                className={css.contactSellerWrapper}
                                style={{ marginTop: '20px' }}
                            >
                                <ContactTheSeller digisellerId={gameSession.digisellerId} />
                            </div>
                        </div>
                    </div>
                </Area>
            )}

            {showOrderClosed && (
                <Area>
                    <div className={css.activationExpErapper}>
                        <div className={css.title}>{tOrderClosed('closed')}</div>
                        <div className={css.text}>{tOrderClosed('message')}</div>
                        <ContactTheSeller digisellerId={gameSession.digisellerId} />
                    </div>
                </Area>
            )}

            {showTempInviteBan && (
                <Area
                    title={`${tCommon('order')} #${gameSession.id} - ${tTempInviteBan(
                        'error'
                    )}`}
                >
                    <div className={css.ivitationRefused}>
                        {!checkCodeLoading && (
                            <>
                                <div className={css.hints}>
                                    <div className={css.hint}>
                                        {tTempInviteBan('youRefused')}
                                    </div>
                                    <div
                                        className={css.hint}
                                        dangerouslySetInnerHTML={{
                                            __html: tTempInviteBan('info', {
                                                botName: `<a href="${gameSession.botInvitationUrl ||
                                                    gameSession.botProfileUrl
                                                    }" 
                                     style="color: #8615BC; text-decoration: none; " 
                                     target="_blank" >${gameSession.botName
                                                    }</a>`,
                                            }),
                                        }}
                                    ></div>
                                </div>

                                <div className={css.accButtons}>
                                    <div className={css.line1}>
                                        {!gameSession.blockOrder && (
                                            <Button
                                                text={tOrderState('changeAccountBut')}
                                                style={{
                                                    backgroundColor: '#FFFFFF',
                                                    color: '#8615BC',
                                                    border: '1px solid #571676',
                                                    marginRight: '1.5em'
                                                }}
                                                onClick={() => {
                                                    apiResetSteamAcc();
                                                }}
                                            />)}
                                        <Button
                                            text={tTempInviteBan('iSentInvitation')}
                                            className={css.but}
                                            onClick={() => {
                                                apiCheckFriend(gameSession.uniqueCode);
                                            }}
                                        />
                                    </div>
                                    {gameSession.isAnotherBotExists && (
                                        <div className={css.line1} style={{ marginTop: '32px' }}>
                                            <Button
                                                text={tTempInviteBan('tryWithBot')}
                                                className={css.but}
                                                style={{ width: '377px' }}
                                                onClick={() => {
                                                    apiResetBot();
                                                }}
                                            />
                                        </div>
                                    )}

                                    <div className={css.contactSellerWrapper}>
                                        <ContactTheSeller digisellerId={gameSession.digisellerId} />
                                    </div>
                                </div>

                                <Dlc isDlc={isDlc} />
                                <Timer endTime={discountEndDate} />
                            </>
                        )}
                        {checkCodeLoading && <CircularLoader color={'#571676'} />}
                    </div>
                </Area>
            )}

        </div>
    );
};

const ContactTheSeller = ({ digisellerId }) => {
    const { t: tActTimeExpires } = useTranslation('activationTimeExpires');

    return (
        <div className={css.contactWrapper}>
            <div
                className={css.contact}
                onClick={() => {
                    let url = `https://digiseller.market/info/buy.asp?id_i=${digisellerId}&lang=ru-RU`;
                    window.open(url, '_blank');
                }}
            >
                {tActTimeExpires('contactTheSeller')}
            </div>
            <div className={css.dash}></div>
        </div>
    );
};

const CheckEditionVersion = () => {
    const { t: tGameSended } = useTranslation('gameSended');

    return (
        <div className={css.checkEditionVersionWrapper}>
            <div
                className={css.checkEditionVersion}
                onClick={() => {
                    window.open(`https://store.steampowered.com/account/licenses/`, '_blank');
                }}
            >
                {tGameSended('checkEditionVersion')}
            </div>
            <div className={css.dash}></div>
        </div>
    );
};

const Area = ({ height, width, title, children, onClose }) => {
    return (
        <div style={{ height, width }} className={css.area}>
            {title && <div className={css.title}>{title}</div>}
            {children}
            {onClose && (
                <div className={css.closeBtn}>
                    <img
                        src={close}
                        onClick={() => {
                            if (onClose) onClose();
                        }}
                    />
                </div>
            )}
        </div>
    );
};

const Dlc = ({ isDlc }) => {
    const { t: tDlc } = useTranslation('dlc');

    if (!isDlc) return null;

    return (
        <div className={css.dlcWrapper}>
            <div className={css.info}>
                <span style={{ color: '#8615BC' }}>DLC | </span>
                {tDlc('info')}
            </div>
        </div>
    );
};

const Timer = ({ endTime }) => {
    const [timer, setTimer] = useState(null);
    const { t: tTimer } = useTranslation('timer');

    useEffect(() => {
        let t = setInterval(() => {
            let now = new Date();

            if (endTime && moment(endTime).diff(now) > 0) {
                let deadline = moment(endTime);
                let last = moment.duration(deadline.diff(moment()));

                //console.log('NOW', moment(new Date()));
                //console.log('deadline -> ', deadline);

                let str = `${Math.trunc(last.asHours())}:${last
                    .minutes()
                    .toString()
                    .padStart(2, '0')}:${last.seconds().toString().padStart(2, '0')}`;
                setTimer(str);
            } else {
                setTimer(null);
            }
        }, 1000);

        return () => {
            clearInterval(t);
        };
    });

    if (!timer) return null;

    return (
        <div className={css.timer}>
            <div>
                {tTimer('title')}: {timer}
            </div>
            <div className={css.info}>
                <HtmlTooltip
                    title={
                        <div
                            style={{
                                padding: '18px 0 18px 0',
                                fontSize: '10px',
                                lineHeight: '130%',
                                textAlign: 'center',
                            }}
                        >
                            {tTimer('expInfoTooltip')}
                        </div>
                    }
                >
                    <img src={info} />
                </HtmlTooltip>
            </div>
        </div>
    );
};

const InputWithButton = ({ butName, onClick, placeholder, defaultValue }) => {
    const [text, setText] = React.useState('');

    useEffect(() => {
        setText(defaultValue);
    }, [defaultValue]);

    return (
        <div className={css.input}>
            <input
                //defaultValue={defaultValue}
                value={text}
                onChange={(e) => {
                    //setManualNumber(e.target.value);
                    setText(e.target.value.trim());
                }}
                placeholder={placeholder}
            />
            <div
                className={css.inBtn}
                onClick={() => {
                    if (onClick) onClick(text);
                }}
            >
                {butName}
            </div>
        </div>
    );
};



const HtmlTooltip = styled(({ className, ...props }) => (
    <Tooltip {...props} classes={{ popper: className }} />
))(({ theme }) => ({
    [`& .${tooltipClasses.tooltip}`]: {
        backgroundColor: '#131213',
        color: '#FFFFFF',
        width: 274,
        //padding: '13px 20px',
        //fontSize: theme.typography.pxToRem(12),
        borderRadius: '15px',
    },
}));

export default OrderState;
