import React, { useEffect, useState } from 'react';
import css from './styles.scss';
import pagArrowLeft from '../../../icons/pagArrowLeft.svg';
import pagArrowRight from '../../../icons/pagArrowRight.svg';
import pagDoubleArrowLeft from '../../../icons/pagDoubleArrowLeft.svg';
import pagDoubleArrowRight from '../../../icons/pagDoubleArrowRight.svg';

const paggination = ({ val = 1, min = 1, max = 1, onChange }) => {
  const [manulNumber, setManualNumber] = useState(val);
  const onChangePage = (newVal) => {
    console.log('newVal', newVal);
    if (newVal > 0 && newVal <= max) onChange(newVal);
  };

  useEffect(() => {
    setManualNumber(val);
  }, [val]);

  return (
    <div className={css.wrapper}>
      <div className={css.input}>
        <input
          defaultValue={manulNumber}
          value={manulNumber}
          type="number"
          step={1}
          onChange={(e) => {
            setManualNumber(e.target.value);
          }}
        />
        <div
          className={css.goBtn}
          onClick={() => {
            onChangePage(Number(manulNumber));
          }}
        >
          GO
        </div>
      </div>

      <div className={css.pages}>
        <div style={{ marginRight: '13px' }}>
          <img
            src={pagDoubleArrowLeft}
            onClick={() => {
              onChangePage(1);
            }}
          />
        </div>
        <div>
          <img
            src={pagArrowLeft}
            onClick={() => {
              onChangePage(Number(val) - 1);
            }}
          />
        </div>
        <div className={css.text}>{`${val} из ${max}`}</div>
        <div style={{ marginRight: '13px' }}>
          <img
            src={pagArrowRight}
            onClick={() => {
              onChangePage(Number(val) + 1);
            }}
          />
        </div>
        <div>
          <img
            src={pagDoubleArrowRight}
            onClick={() => {
              onChangePage(max);
            }}
          />
        </div>
      </div>
    </div>
  );
};

export default paggination;
