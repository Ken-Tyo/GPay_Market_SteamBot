import React from 'react';
import css from './styles.scss';

const TextBox = ({ hint, onChange, defaultValue, cymbol, width }) => {
  const onChangeText = (event) => {
    let val = event.target.value;
    if (onChange) onChange(val);
  };

  return (
    <div
      className={css.wrapper}
      onChange={onChangeText}
      style={{ width: width }}
    >
      <div className={css.inputControl}>
        <div className={css.inputArea}>
          <input type={'text'} defaultValue={defaultValue} />
          {cymbol && <div className={css.cymbol}>{cymbol}</div>}
        </div>
      </div>
      {hint && <div className={css.hint}>{hint}</div>}
    </div>
  );
};

export default TextBox;
