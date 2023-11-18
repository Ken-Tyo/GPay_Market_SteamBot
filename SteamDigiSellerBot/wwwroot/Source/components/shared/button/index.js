import React from 'react';
import css from './styles.scss';

const Button = ({ text, onClick, width, height, style }) => {
  return (
    <div
      style={{
        width: width || 221,
        height: height || 65,
        ...(style || {}),
      }}
      className={css.wrapper + ' ' + css.pointer}
      onClick={() => {
        if (onClick) onClick();
      }}
    >
      {text}
    </div>
  );
};

export default Button;
