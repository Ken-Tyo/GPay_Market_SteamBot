import React from 'react';
import css from './styles.scss';

const TextBox = ({ hint, onChange, defaultValue, symbol, width, height: height, className, symbolStyle }) => {
  const onChangeText = (event) => {
    let val = event.target.value;
    if (onChange) onChange(val);
  };

  return (
    <div
      className={css.wrapper}
      onChange={onChangeText}
      style={{ width: width, height: height ? height : '51px' }}
    >
      <div className={css.inputControl} style={{ height: height ? height : '51px' }}>
        <div className={css.inputArea} style={{ height: height ? height : '51px' }}>
          <input type={'text'} defaultValue={defaultValue} className={className ? className : ''} />
          {symbol && <div className={css.symbol} style={{ ...symbolStyle }}>{symbol}</div>}
        </div>
      </div>
      {hint && <div className={css.hint}>{hint}</div>}
    </div>
  );
};

export default TextBox;
