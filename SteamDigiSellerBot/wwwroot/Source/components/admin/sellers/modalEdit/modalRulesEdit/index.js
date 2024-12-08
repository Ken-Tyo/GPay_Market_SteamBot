import React, { useState, useEffect } from 'react';
import Button from '../../../../shared/button';
import ModalBase from '../../../../shared/modalBase';
import TextBox from '../textbox';
import Switch from '../switch';
import Select from './../select';
import { paramToName, permissionsGetValueActions, permissionsSetValueActions } from '../rules'
import css from './styles.scss';
import { state } from '../../../../../containers/admin/state';

const ModalRulesEdit = ({
  isOpen,
  onClose,
  item,
  isLiveEdit
}) => {
  if (!isOpen) {
    return;
  }

  const initial = [{}];
  const [errors, setErrors] = useState([]);

  const handleSwitch = (key, val) => {
    permissionsSetValueActions.get(key)(item, val)
  };

  const handlePercentChange = (key) => (val) => {
    permissionsSetValueActions.get(key)(item, val)
  };

  const renderDirectBotsDepositPercent = (key, item) => {
    if (key == 'permissionDirectBotsDeposit') {
      let percentKey = `${key}Percent`
      let value = permissionsGetValueActions.get(percentKey)(item)

      return (<TextBox
        onChange={handlePercentChange(percentKey)}
        defaultValue={value}
        symbol={'%'}
        width={62}
        height={28}
        className={css.percentValue}
        symbolStyle={{ padding: '0 5px 0 0', minWidth: 'auto', fontSize: '14px' }} />)
    }

    return null;
  }

  const renderSwitch = (key) => {
    let value = permissionsGetValueActions.get(key)(item)

    return (
      <div className={css.switchContent}>
        <div>{paramToName.get(key)}</div>
        <div className={css.switch}>
          {renderDirectBotsDepositPercent(key, item)}
          <Switch
            value={value}
            onChange={(val) => handleSwitch(key, val)}
            style={{ transform: 'scale(0.75)', marginLeft: '8px' }}
          />
        </div>
      </div>
    )
  }

  const renderSwitches = () => {
    if (!item)
      return;

    return Array.from(paramToName).map((paramToNameItem) => {
      return renderSwitch(paramToNameItem[0])
    })
  }

  const update = async (dto) => {
    const headers = new Headers();
    headers.append("Content-Type", "application/json");
    headers.append("Content-Length", JSON.stringify(dto).length);

    if (dto.rentDays === '-') {
      dto.rentDays = null
    }

    const options = {
      method: "PUT",
      headers: headers,
      body: JSON.stringify(dto)
    }

    let res = await fetch(`/sellers/`, options);

    if (res.ok) {
      const json = await res.json();

      return json;
    }

    return null;
  };

  return (
    <ModalBase
      isOpen={isOpen}
      width={554}
      height={783}
    >
      {errors.length === 0 && (
        <>
          <div className={css.content}>
            <h2 className={css.title}>Права доступа</h2>
            <div className={css.loginTitle}>Логин продавца</div>
            <div className={css.switchWrapper}>
              {renderSwitches()}
            </div>
          </div>
          <Button
            text={'Назад'}
            onClick={() => {
              if (isLiveEdit === true) {
                update(item)
              }

              onClose(item);
            }}
            style={{
              backgroundColor: '#9A7AA9',
              margin: '32px auto 0 auto',
              width: '334px',
            }}
          />
        </>
      )}
    </ModalBase>
  );
};

export default ModalRulesEdit;
