import React from 'react';
import css from './styles.scss';

const Button = ({ children, text, onClick, width, height, style, isDisabled, className, innerTextMargin }) => {
  return (
    <div
      style={{
        width: width || 221,
        minWidth: width || 221,
        height: height || 65,
        ...(style || {}),
      }}
      className={css.wrapper + ' ' + css.pointer + (isDisabled ? css.disabled : '') + (className ? ' ' + className : '') }
      onClick={() => {
        if (onClick && isDisabled !== true) onClick();
      }}
    >
      <span style={{ margin: innerTextMargin ? innerTextMargin : '0px' }}>{text}</span>
      {children}
    </div>
  );
};

export default Button;
