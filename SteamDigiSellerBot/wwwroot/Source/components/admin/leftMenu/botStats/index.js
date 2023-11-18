import React from 'react';
import Section from '../../section';
import css from './styles.scss';

const BotStats = ({ data }) => {
  let val = {
    1: {
      name: 'Активных',
      count: 0,
      color: '#3C965A',
    },
    2: {
      name: 'Временный лимит',
      count: 0,
      color: '#AD8D1D',
    },
    3: {
      name: 'Лимит (Отключен)',
      count: 0,
      color: '#A09F9B',
    },
    4: {
      name: 'Заблокированных',
      count: 0,
      color: '#CA2929',
    },
  };

  (data || []).forEach((bot) => {
    if (!bot.state) return;

    val[bot.state].count++;
  });

  let states = [];
  for (let prop in val) states.push(val[prop]);

  return (
    <div className={css.wrapper}>
      <div className={css.botStats}>
        {states.map((s) => {
          return (
            <div className={css.statItem}>
              <div
                className={css.dot}
                style={{ backgroundColor: `${s.color}` }}
              ></div>
              <div className={css.name}>
                <div>{s.name}</div>

                <div className={css.count}>{s.count}</div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default BotStats;
