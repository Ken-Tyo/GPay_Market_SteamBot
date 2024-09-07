import React from 'react';
import css from './styles.scss';

const TextBox = ({ onChange, defaultValue, width, placeholder, className }) => {
  const onChangeText = (event) => {
    let val = event.target.value;
    if (onChange) onChange(val);
  };

  const wrapperStyle = {
    width: width || '226px',
  };

  return (
    <div className={css.wrapper + className ? css.className : ''} onChange={onChangeText} style={wrapperStyle}>
      <input
        type={'text'}
        defaultValue={defaultValue}
        placeholder={placeholder}
      />
    </div>
  );
};

export default TextBox;
