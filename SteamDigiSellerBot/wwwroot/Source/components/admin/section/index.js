import React, { useEffect } from 'react';
//import css from './styles.scss';

const Section = ({
  children,
  bgcolor,
  height,
  width,
  className,
  styles,
  onClick,
}) => {
  const style = {
    backgroundColor: bgcolor,
    borderRadius: '34px',
    height: height || '65px',
    width: width,
    ...styles,
  };
  return (
    <div
      style={style}
      className={className}
      onClick={() => {
        if (onClick) onClick();
      }}
    >
      {children}
    </div>
  );
};

export default Section;
