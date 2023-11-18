import React, { useState, useEffect } from 'react';
import Button from '../../../shared/button';
import ModalBase from '../../../shared/modalBase';
import TextBox from '../../../shared/textbox';
import css from './styles.scss';

const ModalDigisellerEdit = ({ isOpen, value, onCancel, onSave }) => {
  const initialVal = {
    digisellerApiKey: null,
    digisellerId: null,
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
      title={'Сменить API ключ'}
      width={705}
      height={250}
    >
      <div className={css.content}>
        <div className={css.boxes}>
          <div style={{ marginRight: '17px' }}>
            <TextBox
              onChange={handleChange('digisellerApiKey')}
              defaultValue={val.digisellerApiKey}
              width={303}
              placeholder={'введите новый API ключ digiseller...'}
            />
          </div>
          <div>
            <TextBox
              onChange={handleChange('digisellerId')}
              defaultValue={val.digisellerId}
              width={158}
              placeholder={'введите sellerID...'}
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

export default ModalDigisellerEdit;
