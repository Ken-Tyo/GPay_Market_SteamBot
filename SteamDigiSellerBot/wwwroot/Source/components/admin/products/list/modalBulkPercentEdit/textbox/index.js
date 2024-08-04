import React from 'react';
import css from './styles.scss';

const TextBox = ({ hint, onChange, defaultValue, cymbol, width, fontSize, paddingLeft }) => {
  const onChangeText = (event) => {
    let val = event.target.value;
    if (onChange) onChange(val);
  };

  return (
    <div className={css.wrapper} onChange={onChangeText} style={{
      width: width ?? '112px'      
    }}>
      <div className={css.inputControl}>
        <div className={css.inputArea}>
          <input type={'text'} defaultValue={defaultValue} style={{
            fontSize: fontSize ?? '14px',
            paddingLeft: paddingLeft ?? '15px'
          }} />
          {cymbol && <div className={css.cymbol}>{cymbol}</div>}
        </div>
      </div>
      {hint && <div className={css.hint}>{hint}</div>}
    </div>
  );
};

export default TextBox;
