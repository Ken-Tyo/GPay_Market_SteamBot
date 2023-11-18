import React, { useState } from 'react';
import Button from '../button';
import ModalBase from '../modalBase';
import css from './styles.scss';

const modalSaveText = ({
  title,
  placeholder,
  isOpen,
  onCancel,
  onSave,
  width,
  height,
  defaultValue,
}) => {
  const initialVal = '';
  const [val, setVal] = useState(initialVal);

  return (
    <ModalBase
      isOpen={isOpen}
      title={title}
      width={width || 705}
      height={height || 527}
    >
      <div className={css.content}>
        <div className={css.boxes}>
          <div>
            <textarea
              onInput={(e) => {
                setVal(e.target.value);
              }}
              rows="18"
              class={css.textarea}
              placeholder={placeholder}
              defaultValue={defaultValue}
            ></textarea>
          </div>
        </div>
      </div>

      <div className={css.actions}>
        <Button
          text={onSave.label}
          style={{
            backgroundColor: '#478C35',
            marginRight: '36px',
            width: '284px',
          }}
          onClick={() => {
            if (onSave.action) onSave.action(val);
            setVal('');
          }}
        />
        <Button
          text={onCancel.label || 'Отмена'}
          onClick={() => {
            if (onCancel.action) onCancel.action();
          }}
          style={{ backgroundColor: '#9A7AA9', marginLeft: '0px' }}
        />
      </div>
    </ModalBase>
  );
};

export default modalSaveText;
