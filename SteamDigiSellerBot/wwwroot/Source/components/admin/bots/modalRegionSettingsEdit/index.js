import React, { useState, useEffect } from 'react';
import Button from '../../../shared/button';
import ModalBase from '../../../shared/modalBase';
import TextBox from './textbox';
import CircularProgress from '@mui/material/CircularProgress';
import css from './styles.scss';
import Select from '../../../shared/select';

const FromItemText = ({ name, onChange, hint, value, cymbol }) => {
  return (
    <div className={css.formItem}>
      <div className={css.name}>{name}</div>
      <div>
        <TextBox
          hint={hint}
          onChange={onChange}
          defaultValue={value}
          cymbol={cymbol}
        />
      </div>
    </div>
  );
};

const FormItemSelect = ({
  name,
  onChange,
  hint,
  value,
  options,
  height,
  width,
}) => {
  return (
    <div className={css.formItem}>
      <div className={css.name} style={{ paddingTop: '5px' }}>
        {name}
      </div>
      <div>
        <Select
          options={options}
          height={height}
          width={width}
          onChange={onChange}
          defaultValue={value}
        />
      </div>
    </div>
  );
};

const ModalEdit = ({
  isOpen,
  value,
  onCancel,
  onSave,
  response,
  resetResponse,
  data,
}) => {
  const initialValue = {
    giftSendSteamCurrencyId: 'Китай (CNY)',
    previousPurchasesJPY: 0,
    previousPurchasesCNY: 0,
    previousPurchasesSteamCurrencyId: '-',
  };
  const [item, setItem] = useState(initialValue);

  useEffect(() => {
    if (value)
      if (!isOpen) {
        // setItem({
        //   ...value,
        //   proxy: value.proxyStr ? value.proxyStr : '',
        // });

        setItem(initialValue);
      }
  }, [value, isOpen]);

  const handleChange = (prop) => (val) => {
    setItem({ ...item, [prop]: val });
    console.log(val);
  };

  const modalHeight = 580;

  let hasProblemPur = data.hasProblemPurchase;
  let hasProblemReg = data.region === 'CN' || data.region === 'JP';
  let str1 = 'регион';
  if (hasProblemPur && hasProblemReg) {
    str1 = 'регион';
  } else if (hasProblemPur && !hasProblemReg) {
    str1 = 'валюта';
  }

  let regionName = 'Китай / Япония';
  if (data.region === 'CN') regionName = 'Китай';
  else if (data.region === 'JP') regionName = 'Япония';

  return (
    <ModalBase
      isOpen={isOpen}
      title={'Настройка бота'}
      width={554}
      height={modalHeight}
    >
      {!response.loading && response.errors.length === 0 && (
        <>
          <div className={css.content}>
            <div
              style={{
                fontSize: '14px',
                display: 'flex',
                justifyContent: 'center',
              }}
            >
              <span>
                Проблемный {str1}:{' '}
                <span style={{ color: '#B93ED8' }}> {regionName}</span> !
              </span>
            </div>
            <div className={css.fields}>
              <FormItemSelect
                name={'Планируемая страна отправки гифтов:'}
                height={75}
                width={199}
                options={[{ name: 'Китай (CNY)' }, { name: 'Япония (JPY)' }]}
                value={item.giftSendSteamCurrencyId}
                onChange={handleChange('giftSendSteamCurrencyId')}
              />
              <div className={css.formItem}>
                <div
                  className={css.name}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    paddingTop: '0px',
                  }}
                >
                  Введите сумму совершенных ранее личных покупок:
                </div>
                <div>
                  <div style={{ marginBottom: '12px' }}>
                    <TextBox
                      onChange={handleChange('previousPurchasesJPY')}
                      cymbol={'JPY'}
                      value={0}
                    />
                  </div>
                  <div>
                    <TextBox
                      onChange={handleChange('previousPurchasesCNY')}
                      cymbol={'CNY'}
                      value={0}
                    />
                  </div>
                </div>
              </div>
              <div
                style={{
                  display: 'flex',
                  justifyContent: 'center',
                  fontSize: '14px',
                  paddingBottom: '23px',
                }}
              >
                Либо
              </div>
              <FormItemSelect
                name={'Ранее личные покупки совершались в:'}
                height={105}
                width={199}
                options={[{ name: '-' }, { name: 'CNY' }, { name: 'JPY' }]}
                value={item.previousPurchasesSteamCurrencyId}
                onChange={handleChange('previousPurchasesSteamCurrencyId')}
              />
            </div>
          </div>

          <div className={css.footer}>
            <div className={css.actions}>
              <Button
                text={'Сохранить'}
                style={{
                  backgroundColor: '#478C35',
                  marginRight: '24px',
                  width: '271px',
                }}
                onClick={() => {
                  let i = { ...item };
                  i.giftSendSteamCurrencyId =
                    i.giftSendSteamCurrencyId === 'Китай (CNY)' ? 23 : 8;

                  if (i.previousPurchasesSteamCurrencyId === '-')
                    i.previousPurchasesSteamCurrencyId = null;
                  else
                    i.previousPurchasesSteamCurrencyId =
                      i.previousPurchasesSteamCurrencyId === 'CNY' ? 23 : 8;

                  onSave(i);
                }}
              />
              <Button
                text={'Отмена'}
                onClick={() => {
                  if (onCancel) onCancel();
                }}
                style={{
                  backgroundColor: '#9A7AA9',
                  marginLeft: '0px',
                  width: '183px',
                }}
              />
            </div>
          </div>
        </>
      )}
      {!response.loading && response.errors.length > 0 && (
        <>
          <div className={css.errors}>
            <div className={css.title}>
              <div>Ошибка при добавлении!</div>
            </div>
            <div className={css.list}>
              {response.errors.map((e) => {
                return <div className={css.item}>- {e}</div>;
              })}
            </div>
          </div>
          <div className={css.footer}>
            <div className={css.actions}>
              <Button
                text={'Закрыть'}
                onClick={() => {
                  if (resetResponse) resetResponse();
                }}
                style={{
                  backgroundColor: '#9A7AA9',
                  marginLeft: '0px',
                  width: '226px',
                }}
              />
            </div>
          </div>
        </>
      )}
      {response.loading && (
        <div className={css.loading}>
          <CircularProgress
            color="inherit"
            sx={{
              height: '99px !important',
              width: '99px !important',
            }}
          />
        </div>
      )}
    </ModalBase>
  );
};

export default ModalEdit;
