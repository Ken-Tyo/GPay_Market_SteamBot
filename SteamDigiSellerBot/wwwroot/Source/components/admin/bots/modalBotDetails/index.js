import React, { useEffect, useState } from 'react';
import Button from '../../../shared/button';
import ModalBase from '../../../shared/modalBase';
import css from './styles.scss';
import copy from 'copy-to-clipboard';

const modalBotDetails = ({ isOpen, onCancel, data }) => {
  const initialVal = {};
  const [val, setVal] = useState(initialVal);

  useEffect(() => {
    setVal(data);
  }, [data]);

  let states = {
    1: {
      name: 'Активен',
      color: '#3C965A',
    },
    2: {
      name: 'Временный лимит',
      color: '#AD8D1D',
    },
    3: {
      name: 'Лимит (Отключен)',
      color: '#A09F9B',
    },
    4: {
      name: 'Заблокирован',
      color: '#CA2929',
      }
  };

  const renderItem = (name, val, onClick) => {
    return (
      <div className={css.item}>
        <div className={css.name}>{name}</div>
        <div
          className={css.val + ' ' + (onClick ? css.copyable : '')}
          onClick={() => {
            if (onClick) onClick(val);
          }}
        >
          {val}
        </div>
      </div>
    );
  };

  const copyToClipboard = (str) => {
    //if (str) navigator.clipboard.writeText(str);
    copy(str);
  };

  return (
    <ModalBase
      isOpen={isOpen}
      title={'Подробнее о боте'}
      width={554}
      height={768}
    >
      <div className={css.content}>
        <div className={css.boxes}>
          <div className={css.infoarea}>
            {renderItem('Login:', val.userName, copyToClipboard)}
            {renderItem('Password:', val.password, copyToClipboard)}
            {renderItem('Proxy:', val.proxyStr, copyToClipboard)}
            {renderItem('User Agent:', val.userAgent, copyToClipboard)}
            {renderItem(
              'Состояние:',
              val.state && (
                <div className={css.botState}>
                  <div
                    className={css.dot}
                    style={{ backgroundColor: `${states[val.state].color}` }}
                  ></div>
                  <div className={css.stateName}>{states[val.state].name}</div>
                </div>
              )
            )}
          </div>
        </div>
      </div>

      <div className={css.actions}>
        <Button
          text={'Закрыть'}
          onClick={() => {
            if (onCancel) onCancel();
          }}
          style={{ backgroundColor: '#9A7AA9', marginLeft: '0px' }}
        />
      </div>
    </ModalBase>
  );
};

export default modalBotDetails;
