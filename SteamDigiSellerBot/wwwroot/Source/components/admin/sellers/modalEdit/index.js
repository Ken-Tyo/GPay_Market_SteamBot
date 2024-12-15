import React, { useState, useEffect } from 'react';
import Button from '../../../shared/button';
import ModalBase from '../../../shared/modalBase';
import Switch from './switch';
import Colors from './switch/colors'
import ModalRulesEdit from './modalRulesEdit'
import TextBox from './textbox';
import TextSwitch from './textSwitch';
import Select from './select';
import css from './styles.scss';
import { initial, initPermissions, paramToName, permissionsGetValueActions, permissionsSetValueActions } from './rules'
import { state } from '../../../../containers/admin/state';

const FromItemText = ({ name, onChange, hint, value, symbol, className }) => {
  return (
    <div className={css.formItem}>
      <div className={css.name}>{name}</div>
      <div>
        <TextBox
          hint={hint}
          onChange={onChange}
          defaultValue={value}
          symbol={symbol}
          className={className}
        />
      </div>
    </div>
  );
};

const ModalEdit = ({
  isOpen,
  onClose,
  user,
}) => {
  if (!isOpen)
    return;
  
  const [item, setItem] = useState(initial);
  const [isModalRulesEditOpen, setIsModalRulesEditOpen] = useState(false)
  const [errors, setErrors] = useState([])
  const [isLoading, setIsLoading] = useState(false)
  const [itemForPermissions, setItemForPermissions] = useState({})

  useEffect(() => {
    if (!user) {
      let itemPermissions = {}
      Object.assign(itemPermissions, item)
      setItemForPermissions(itemPermissions)

      return;
    }

    if (user.rentDays == null) {
      user.rentDays = initial.rentDays
    }

    setItem(user)

    let itemPermissions = {}
    Object.assign(itemPermissions, user)
    setItemForPermissions(itemPermissions)
  }, [])

  const handleChange = (prop) => (val) => {
    setItem({ ...item, [prop]: val });
  };

  const getTitle = () => {
    if (!user) {
      return 'Создать продавца'
    }

    return 'Редактировать продавца'
  }

  const getButtonText = () => {
    if (!user) {
      return 'Добавить'
    }

    return 'Сохранить'
  }

  const create = async (dto) => {
    const headers = new Headers();
    headers.append("Content-Type", "application/json");
    headers.append("Content-Length", JSON.stringify(dto).length);

    if (dto.rentDays === '-') {
      dto.rentDays = null
    }

    const options = {
      method: "POST",
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

  const save = async (dto) => {
    if (dto.id && dto.id > 0) {
      return await update(dto)
    } else {
      return await create(dto)
    }
  }

  const renderBlock = () => {
    if (user) {
      return <div className={css.blocked}>
        <div>Заблокирован</div>
        <div className={css.switchContainer}>
          <Switch
            value={item.blocked}
            onChange={(val) => item.blocked = !item.blocked}
            style={{ transform: 'scale(0.75)', margin: '0 auto' }}
            color={Colors.ORANGE}
          />
        </div>
      </div>
    }
  }

  const getWindowHeight = () => {
    return user ? 673 : 625
  }

  return (
    <ModalBase
      isOpen={isOpen}
      title={getTitle()}
      width={554}
      height={getWindowHeight()}
    >
      {!isLoading && errors.length > 0 && (
        <>
          <div className={css.errors}>
            <div className={css.title}>
              <div>Ошибка при добавлении!</div>
            </div>
            <div className={css.list}>
              {errors.map((e) => {
                return <div className={css.item}>- {e}</div>;
              })}
            </div>
          </div>
        </>
      )}

      {!isLoading && (
        <>
          <ModalRulesEdit
            isOpen={isModalRulesEditOpen}
            onClose={(editedItem) => {
              let current = item
              for (let keyValuePair of paramToName) {
                var val = permissionsGetValueActions.get(keyValuePair[0])(editedItem)
                permissionsSetValueActions.get(keyValuePair[0])(current, val)
              }

              setItem(current)
              setIsModalRulesEditOpen(false)
            }}
            isLiveEdit={true}
            title=''
            item={itemForPermissions}
            isLoading={true}
          />

          <div className={css.content}>
            <FromItemText
              name={'Логин:'}
              onChange={handleChange('login')}
              value={item.login}
            />

            <FromItemText
              name={'Пароль:'}
              onChange={handleChange('password')}
              value={item.password}
            />

            <FromItemText
              name={'Аренда:'}
              symbol={'дней'}
              onChange={handleChange('rentDays')}
              value={item.rentDays}
            />

            <FromItemText
              name={'Лимит товаров:'}
              symbol={'шт.'}
              onChange={handleChange('itemsLimit')}
              value={item.itemsLimit}
            />

            { renderBlock() }

            <Button
              text={'Настроить права'}
              width={384}
              height={51}
              style={{
                fontSize: '16px'
              }}
              className={css.changeAccessRights}
              onClick={() => {
                setIsModalRulesEditOpen(true);
              }}
            />
          </div>

          <div className={css.actions}>
            <Button
              text={getButtonText()}
              style={{
                backgroundColor: '#478C35',
                marginRight: '24px',
              }}
              width={271}
              height={65}
              onClick={() => {
                save(item).then(res => {
                  if (res.errorText && res.errorText != '') {
                    setErrors([res.errorText])
                  }
                  else {
                    if (onClose) onClose(item);
                  }
                })
              }}
            />
            <Button
              width={183}
              height={65}
              text={'Отмена'}
              onClick={async () => {
                if (onClose) onClose();
                setItem(initial);
              }}
              style={{ backgroundColor: '#9A7AA9', marginLeft: '0px' }}
            />
          </div>
        </>
      )}
    </ModalBase>
  );
};

export default ModalEdit;
