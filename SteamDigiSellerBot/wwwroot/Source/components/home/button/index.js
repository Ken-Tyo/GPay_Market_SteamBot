import React from 'react';
import css from './styles.scss';

const Button = ({ text, onClick, style, className }) => {
  return (
    <div
      style={{
        ...(style || {}),
      }}
      className={css.wrapper + ' ' + css.pointer + ' ' + (className || '')}
      onClick={() => {
        if (onClick) onClick();
      }}
    >
      {text}
    </div>
  );
};

export default Button;
