import React, { useState, useEffect } from 'react';
import Button from '../../../shared/button';
import ModalBase from '../../../shared/modalBase';
import TextBox from '../../../shared/textbox';
import css from './styles.scss';

const ModalChangePassword = ({ isOpen, value, onCancel, onSave }) => {
  const initialVal = {
    password: null,
  };
  const [val, setVal] = useState(initialVal);

  useEffect(() => {
    setVal(value);
  }, [value]);

  const handleChange = (prop) => (propVal) => {
    setVal({ ...val, [prop]: propVal });
  };

  return (
    <ModalBase
      isOpen={isOpen}
      title={'Сменить пароль'}
      width={705}
      height={250}
    >
      <div className={css.content}>
        <div className={css.boxes}>
          <div>
            <TextBox
              onChange={handleChange('password')}
              //defaultValue={val.password}
              width={344}
              placeholder={'введите новый пароль для входа в ЛК...'}
            />
          </div>
        </div>
      </div>

      <div className={css.actions}>
        <Button
          text={'Готово'}
          style={{
            backgroundColor: '#478C35',
            marginRight: '36px',
            width: '221px',
          }}
          onClick={() => {
            onSave(val);
            setVal('');
          }}
        />
        <Button
          text={'Отмена'}
          onClick={() => {
            if (onCancel) onCancel();
            setVal(initialVal);
          }}
          style={{ backgroundColor: '#9A7AA9', marginLeft: '0px' }}
        />
      </div>
    </ModalBase>
  );
};

export default ModalChangePassword;
