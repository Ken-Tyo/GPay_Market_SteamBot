import React from 'react';
import Section from '../../../section';
import css from './styles.scss';
import historyImg from '../../../../../icons/history.svg';

const statusBadge = ({ data, onClick }) => {
  return (
    <div className={css.wrapper}>
      <div className={css.nameText} style={{ color: data?.color }}>
        {data?.name}
      </div>
      <div
        className={css.historyWrapper}
        onClick={() => {
          if (onClick) onClick();
        }}
      >
        <img src={historyImg} />
      </div>
    </div>
  );
};

export default statusBadge;
