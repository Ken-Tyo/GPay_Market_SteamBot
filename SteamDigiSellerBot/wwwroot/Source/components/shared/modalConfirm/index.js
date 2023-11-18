import * as React from 'react';
import Button from '../button';
import DialogActions from '@mui/material/DialogActions';
import ModalBase from '../modalBase';
import css from './styles.scss';

export default function ConfirmModal({
  isOpen,
  title,
  content,
  onConfirm,
  onCancel,
  height,
}) {
  let confirmBgStyle = onConfirm?.bg
    ? {
        backgroundColor: onConfirm.bg,
      }
    : {};

  return (
    <div>
      <ModalBase isOpen={isOpen} title={title} height={height}>
        <div className={css.content}>{content}</div>
        <DialogActions
          className={css.actions}
          sx={{ paddingTop: 0, paddingBottom: '0 !important' }}
        >
          <Button
            text={onConfirm?.text || 'Удалить'}
            onClick={() => {
              if (onConfirm?.action) onConfirm.action();
            }}
            style={{ marginRight: '28px', ...confirmBgStyle }}
          />
          <Button
            text={onCancel?.text || 'Отмена'}
            onClick={() => {
              if (onCancel?.action) onCancel.action();
            }}
            style={{ backgroundColor: '#9A7AA9' }}
          />
        </DialogActions>
      </ModalBase>
    </div>
  );
}
