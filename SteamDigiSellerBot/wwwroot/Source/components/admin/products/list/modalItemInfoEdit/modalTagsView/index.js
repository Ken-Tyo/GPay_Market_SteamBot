import React, { useState, useEffect } from 'react';
import Button from '../../../../../shared/button';
import ModalBase from '../../../../../shared/modalBase';
import ModalTypeTagEdit from './ModalTypeTagEdit'
import ModalPromoTagEdit from './ModalPromoTagEdit'
import css from './styles.scss';

const ModalTagsView = ({ isOpen, onClose }) => {
  if (!isOpen)
    return;

  const [showTagTypeEditModal, setShowTagTypeEditModal] = useState(false)
  const [showTagPromoEditModal, setShowTagPromoEditModal] = useState(false)

  const renderModals = () => {
    if (showTagTypeEditModal) {
      return (<ModalTypeTagEdit
        isOpen={showTagTypeEditModal}
        onClose={() => setShowTagTypeEditModal(!showTagTypeEditModal)} />);
    }

    if (showTagPromoEditModal) {
      return (<ModalPromoTagEdit
        isOpen={showTagPromoEditModal}
        onClose={() => setShowTagPromoEditModal(!showTagPromoEditModal)} />);
    }
  }

  return (
    <div>
      <ModalBase
        isOpen={isOpen}
        className={css.tagsViewModalContent}
        width={'730px'}
        height={'auto'}
      >
        <div className={css.title}><h1>Теги шаблонов</h1></div>
        <div className={css.tagButtons}>
          <Button onClick={() => setShowTagTypeEditModal(true)} text={'%type%'} width={'190px'} height={'70px'} />
          <Button onClick={() => setShowTagPromoEditModal(true)} text={'%promo%'} width={'190px'} height={'70px'} />
        </div>
        <Button width={'auto'} height={'auto'} className={css.closeTagViewButton} onClick={onClose}>Закрыть</Button>
      </ModalBase>
      {renderModals()}
    </div>);
}

export default ModalTagsView;