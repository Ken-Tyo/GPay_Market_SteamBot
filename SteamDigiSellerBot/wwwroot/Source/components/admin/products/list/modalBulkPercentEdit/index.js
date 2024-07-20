import React, { useState, useEffect } from 'react';
import Button from '../../../../shared/button';
import ModalBase from '../../../../shared/modalBase';
import TextBox from './textbox';
import css from './styles.scss';
import Select from '../modalEdit/select'

const FromItemText = ({ name_line_1, name_line_2, onChange, hint, value, cymbol }) => {
  return (
    <div className={css.contentItem}>
      <div className={css.name}><div>{name_line_1}</div><div>{name_line_2}</div></div>
      <div>
        <TextBox
          hint={hint}
          onChange={onChange}
          defaultValue={value}
          cymbol={cymbol}
        />
      </div>
    </div>
  );
};

const IncreaseDecreaseItem = ({ name, onValueChange, value, operatorValue, onOperatorChange, symbol }) => {
  const operators = [{ name: '+' }, {name: '-'}];

  return (
    <div className={css.contentItem}>
      <div className={css.name}>{name}</div>
      <div className={css.operatorPercent}>
        <div className={css.operator}>
          <Select
            width={70}
            height={'auto'}
            options={operators}
            defaultValue={operatorValue}
            onChange={onOperatorChange}
          />
        </div>
        <div className={css.percent}>
          <TextBox
            onChange={onValueChange}
            defaultValue={value}
            cymbol={symbol}
          />
        </div>
      </div>
    </div>
  );
};

const ModalBulkPercentEdit = ({ isOpen, onCancel, onSave }) => {
  const operators = [{id: 'Increase', name: '+'}, {id: 'Decrease', name: '-'}]

  const [val, setVal] = useState('');
  const [increaseDecreaseOperator, setIncreaseDecreaseOperator] = useState(operators[0]);
  const [increaseDecreaseVal, setIncreaseDecreaseVal] = useState('');

  const handleChange = (val) => {
    setVal(val);
  };

  const handleIncreaseDecreaseChange = (val) => {
    setIncreaseDecreaseVal(val);
  };

  const handleIncreaseDecreaseOperatorChange = (val) => {
    let selectedOperator = operators.find((o) => o.name === val);
    setIncreaseDecreaseOperator(selectedOperator);
  };

  const clearValues = () => {
    setVal('');
    setIncreaseDecreaseOperator(operators[0]);
    setIncreaseDecreaseVal('');
  }

  return (
    <ModalBase
      isOpen={isOpen}
      title={'Массовая смена цен'}
      width={600}
      height={450}
    >
      <div className={css.content}>
        <FromItemText
          name_line_1={'Введите цену в процентах.'}
          name_line_2={'Цена будет рассчитываться от цены игр в Steam'}
          onChange={handleChange}
          value={val}
          cymbol={'%'}
        />
        <div className={css.itemOr}><span>или</span></div>
        <div>
          <div>
            <IncreaseDecreaseItem
              name={'Изменить цену на'}
              value={increaseDecreaseVal}
              onValueChange={handleIncreaseDecreaseChange}
              operatorValue={increaseDecreaseOperator.name}
              onOperatorChange={handleIncreaseDecreaseOperatorChange}
              symbol={'%'} />
          </div>
        </div>
      </div>

      <div className={css.actions}>
        <Button
          text={'Подтвердить'}
          style={{
            backgroundColor: '#478C35',
            marginRight: '24px',
            width: '271px',
          }}
          onClick={() => {
            onSave(val, increaseDecreaseOperator, increaseDecreaseVal);
            clearValues();
          }}
        />
        <Button
          text={'Отмена'}
          onClick={() => {
            if (onCancel) onCancel();
            clearValues();
          }}
          style={{ backgroundColor: '#9A7AA9', marginLeft: '0px' }}
        />
      </div>
    </ModalBase>
  );
};

export default ModalBulkPercentEdit;
