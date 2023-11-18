import React, { useState } from 'react';
import css from './styles.scss';
import arrowUp from '../../../../../icons/arrowUp.svg';
import arrowDown from '../../../../../icons/arrowDown.svg';

const Expander = ({ header, content }) => {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div className={css.wrapper}>
      <div
        className={css.header}
        onClick={() => {
          setIsOpen(!isOpen);
        }}
      >
        <div className={css.headerContent}>{header}</div>
        <div className={css.arrow}>
          <img src={isOpen ? arrowUp : arrowDown} />
        </div>
      </div>
      {isOpen && <div className={css.content}>{content}</div>}
    </div>
  );
};

export default Expander;
