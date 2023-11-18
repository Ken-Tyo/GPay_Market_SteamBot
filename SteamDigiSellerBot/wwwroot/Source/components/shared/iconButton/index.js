import React from 'react';
import css from './styles.scss';

const iconButton = ({ icon, onClick, disabled, className }) => {
  let disabledStyle = disabled
    ? {
        backgroundColor: '#535353',
        opacity: '80%',
        cursor: 'inherit',
      }
    : {};

  return (
    <div
      className={css.button + ' ' + (className || '')}
      style={{
        ...disabledStyle,
      }}
      onClick={() => {
        if (onClick && !disabled) onClick();
      }}
    >
      <div className={css.icon}>
        <img src={icon}></img>
      </div>
    </div>
  );
};

export default iconButton;
