import React, { useState, useEffect } from 'react';
import Button from '../../../shared/button';
import ModalBase from '../../../shared/modalBase';
import TextBox from './textbox';
import CircularProgress from '@mui/material/CircularProgress';
import css from './styles.scss';
import Switch from '../../../shared/switch';
import { apiBotSetIsReserve } from '../../../../containers/admin/state';

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

const FromItemSwitch = ({ name, value, onChange }) => {
  return (
    <div className={css.formItem}>
      <div className={css.name}>{name}</div>
      <div style={{ marginTop: "10px", marginRight: "60px" }}>
        <Switch
          value={value}
          onChange={onChange}
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
  isReserve,
}) => {
  const initialValue = {
    id: null,
    userName: null,
    password: null,
    proxy: null,
    maFile: null,
    gameSendLimit: null,
    isReserve:null,
  };
  const [item, setItem] = useState(initialValue);

  useEffect(() => {
    if (value)
      setItem({
        ...value,
        proxy: value.proxyStr ? value.proxyStr : '',
      });

    if (!isOpen) {
      setItem(initialValue);
    }
  }, [value, isOpen]);

  const handleChange = (prop) => (val) => {
    setItem({ ...item, [prop]: val });
  };

  const fileRef = React.useRef();
  const modalHeight = item.id ? 835 : 599;

  return (
    <ModalBase
      isOpen={isOpen}
      title={item.id ? 'Редактировать бота' : 'Добавить бота'}
      width={554}
      height={modalHeight}
    >
      {!response.loading && response.errors.length === 0 && (
        <>
          <div className={css.content}>
            <div className={css.fields}>
              <FromItemText
                name={'Логин:'}
                onChange={handleChange('userName')}
                value={item.userName}
              />
              <FromItemText
                name={'Пароль:'}
                onChange={handleChange('password')}
                value={item.password}
              />
              <FromItemText
                name={'Прокси:'}
                hint={'Прокси вводить форматом: ip:port:login:pass'}
                onChange={handleChange('proxy')}
                value={item.proxy}
              />

              {item.id && (
                <FromItemText
                  name={'Доп. значение лимита:'}
                  hint={
                    'Данным параметром Вы можете корректировать максимальный лимит отправки игр. Параметр является необязательным'
                  }
                  onChange={handleChange('gameSendLimitAddParam')}
                  value={item.gameSendLimitAddParam}
                  cymbol={'$'}
                />
              )}
              {item.id && (
                <FromItemSwitch
                  name={'Запасной бот:'} 
                  onChange={(val) => {
                     apiBotSetIsReserve(item.id, val);
                     handleChange('isReserve');
                  }}
                  value={item.isReserve}
                />
              )}
            </div>
          </div>

          <div className={css.footer}>
            <div className={css.maFileBtnwrapper}>
              <Button
                text={'Изменить .MaFile'}
                style={{
                  backgroundColor: '#A348CE',
                  width: '478px',
                }}
                onClick={() => {
                  fileRef.current.click();
                }}
              />
              <input
                ref={fileRef}
                type={'file'}
                hidden={true}
                multiple={false}
                onChange={(e) => {
                  handleChange('maFile')(e.target.files[0]);
                }}
              />
            </div>

            <div className={css.actions}>
              <Button
                text={item.id ? 'Сохранить' : 'Добавить'}
                style={{
                  backgroundColor: '#478C35',
                  marginRight: '24px',
                  width: '271px',
                }}
                onClick={() => {
                  onSave(item);
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
