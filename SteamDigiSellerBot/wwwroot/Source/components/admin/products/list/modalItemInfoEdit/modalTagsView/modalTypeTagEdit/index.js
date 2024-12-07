import React, { useState, useEffect } from 'react';
import Button from '../../../../../../shared/button';
import ModalBase from '../../../../../../shared/modalBase';
import css from './styles.scss';
import TextBox from '../../../../../../shared/textbox';
import { apiTagTypeReplacementValues, apiFetchTagTypeReplacementValues } from '../../../../../../../containers/admin/state'

const ModalTypeTagEdit = ({ isOpen, onClose }) => {
  if (!isOpen)
    return;

  const languageCodes = ['ru-RU', 'en-US'];
  const [tagGameRu, setTagGameRu] = useState('')
  const [tagGameEn, setTagGameEn] = useState('')
  const [tagDlcRu, setTagDlcRu] = useState('')
  const [tagDlcEn, setTagDlcEn] = useState('')

  const languageCodesSetters = new Map()
    .set(languageCodes[0], (isDlc) => isDlc == true ? setTagDlcRu : setTagGameRu)
    .set(languageCodes[1], (isDlc) => isDlc == true ? setTagDlcEn : setTagGameEn)

  const handleChange = (setValue, event) => {
    let val = event.target.value;
    setValue(val);
  };

  const onSave = async () => {
    var data = [
      {
        isDlc: true,
        values: [{
          languageCode: languageCodes[0],
          value: tagDlcRu,
        }, {
          languageCode: languageCodes[1],
          value: tagDlcEn,
        }]
      },
      {
        isDlc: false,
        values: [{
          languageCode: languageCodes[0],
          value: tagGameRu,
        }, {
          languageCode: languageCodes[1],
          value: tagGameEn,
        }]
      }];

    await apiTagTypeReplacementValues(data);
    onClose();
  }

  useEffect(() => {
    apiFetchTagTypeReplacementValues().then((data) => {
      if (!data || data.length == 0) {
        return;
      }

      for (var item of data) {
        if (!item.tagTypeReplacementValues || item.tagTypeReplacementValues.length == 0) {
          continue;
        }

        for (var tagTypeReplacementValue of item.tagTypeReplacementValues) {
          var setter = languageCodesSetters.get(tagTypeReplacementValue.languageCode)(item.isDlc)
          if (!tagTypeReplacementValue.value) {
            continue;
          }
          setter(tagTypeReplacementValue.value)
        }
      }
    })
  }, [])

  return (
    <ModalBase
      isOpen={isOpen}
      className={css.tagTypeEditModalContent}
      width={'685px'}
      height={'auto'}
    >
      <div className={css.title}><h1>Теги - %type%</h1></div>
      <div className={css.titleDescription}>Передает тип продукта - Игра/Дополнение</div>
      <div className={css.tagItemEdit}>
        <div className={css.tagItemEditLang}>
          <div className={css.tagItemEditLangTitle}>
            <svg width="33" height="23" viewBox="0 0 33 23" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M1.77223 0H30.5082C31.4849 0 32.2804 0.793988 32.2804 1.76896V11.2092H0V1.76896C0 0.793988 0.795458 0 1.77223 0Z" fill="white" />
              <path d="M0 11.2095H32.2862V20.6498C32.2862 21.6247 31.4908 22.4187 30.514 22.4187H1.77223C0.795458 22.4187 0 21.6189 0 20.6498V11.2095Z" fill="#D52B1E" />
              <path d="M0 7.47266H32.2862V14.9455H0V7.47266Z" fill="#0039A6" />
            </svg>
            <span>Игра</span>
          </div>
          <div className={css.tagItemValue}>
            <input
              type={'text'}
              defaultValue={tagGameRu}
              value={tagGameRu}
              placeholder={'Игра'}
              onChange={(event) => handleChange(setTagGameRu, event)}
            />
          </div>
        </div>

        <div className={css.tagItemEditLang}>
          <div className={css.tagItemEditLangTitle}>
            <svg width="32" height="23" viewBox="0 0 32 23" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M1.66377 22.2197H30.3362C31.258 22.1676 32 21.3981 32 20.4664V1.75328C32 0.792735 31.2174 0.00578639 30.2551 0H1.74493C0.782609 0.00578639 0 0.792735 0 1.75328V20.4607C0 21.3981 0.742029 22.1676 1.66377 22.2197Z" fill="#FEFEFE" />
              <path d="M13.7623 13.326V22.2197H18.2145V13.326H32V8.8821H18.2145V0H13.7623V8.8821H0V13.326H13.7623Z" fill="#C8102E" />
              <path d="M19.6985 7.19248V0H30.2666C30.997 0.0115728 31.6231 0.468697 31.8782 1.11099L19.6985 7.19248Z" fill="#012169" />
              <path d="M19.6985 15.0273V22.2198H30.3362C31.0376 22.1793 31.6289 21.728 31.8782 21.1088L19.6985 15.0273Z" fill="#012169" />
              <path d="M12.2782 15.0273V22.2198H1.66373C0.962284 22.1793 0.365182 21.728 0.121704 21.0973L12.2782 15.0273Z" fill="#012169" />
              <path d="M12.2782 7.19248V0H1.7333C1.00286 0.0115728 0.370979 0.474484 0.121704 1.12256L12.2782 7.19248Z" fill="#012169" />
              <path d="M0 7.40662H4.43478L0 5.19043V7.40662Z" fill="#012169" />
              <path d="M32 7.40647H27.542L32 5.17871V7.40647Z" fill="#012169" />
              <path d="M32 14.813H27.542L32 17.0407V14.813Z" fill="#012169" />
              <path d="M0 14.813H4.43478L0 17.0292V14.813Z" fill="#012169" />
              <path d="M32 1.88037L20.9565 7.40637H23.4261L32 3.12444V1.88037Z" fill="#C8102E" />
              <path d="M11.0203 14.813H8.55072L0 19.0833V20.3274L11.0435 14.813H11.0203Z" fill="#C8102E" />
              <path d="M6.09855 7.4123H8.56812L0 3.13037V4.36866L6.09855 7.4123Z" fill="#C8102E" />
              <path d="M25.8724 14.8076H23.4028L31.9999 19.1069V17.8686L25.8724 14.8076Z" fill="#C8102E" />
            </svg>
            <span>Игра</span>
          </div>
          <div className={css.tagItemValue}>
            <input
              type={'text'}
              defaultValue={tagGameEn}
              value={tagGameEn}
              placeholder={'Game'}
              onChange={(event) => handleChange(setTagGameEn, event)}
            />
          </div>
        </div>
      </div>

      <div className={css.tagItemEdit}>
        <div className={css.tagItemEditLang}>
          <div className={css.tagItemEditLangTitle}>
            <svg width="33" height="23" viewBox="0 0 33 23" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M1.77223 0H30.5082C31.4849 0 32.2804 0.793988 32.2804 1.76896V11.2092H0V1.76896C0 0.793988 0.795458 0 1.77223 0Z" fill="white" />
              <path d="M0 11.2095H32.2862V20.6498C32.2862 21.6247 31.4908 22.4187 30.514 22.4187H1.77223C0.795458 22.4187 0 21.6189 0 20.6498V11.2095Z" fill="#D52B1E" />
              <path d="M0 7.47266H32.2862V14.9455H0V7.47266Z" fill="#0039A6" />
            </svg>
            <span>DLC</span>
          </div>
          <div className={css.tagItemValue}>
            <input
              type={'text'}
              defaultValue={tagDlcRu}
              value={tagDlcRu}
              placeholder={'Дополнение'}
              onChange={(event) => handleChange(setTagDlcRu, event)}
            />
          </div>
        </div>

        <div className={css.tagItemEditLang}>
          <div className={css.tagItemEditLangTitle}>
            <svg width="32" height="23" viewBox="0 0 32 23" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M1.66377 22.2197H30.3362C31.258 22.1676 32 21.3981 32 20.4664V1.75328C32 0.792735 31.2174 0.00578639 30.2551 0H1.74493C0.782609 0.00578639 0 0.792735 0 1.75328V20.4607C0 21.3981 0.742029 22.1676 1.66377 22.2197Z" fill="#FEFEFE" />
              <path d="M13.7623 13.326V22.2197H18.2145V13.326H32V8.8821H18.2145V0H13.7623V8.8821H0V13.326H13.7623Z" fill="#C8102E" />
              <path d="M19.6985 7.19248V0H30.2666C30.997 0.0115728 31.6231 0.468697 31.8782 1.11099L19.6985 7.19248Z" fill="#012169" />
              <path d="M19.6985 15.0273V22.2198H30.3362C31.0376 22.1793 31.6289 21.728 31.8782 21.1088L19.6985 15.0273Z" fill="#012169" />
              <path d="M12.2782 15.0273V22.2198H1.66373C0.962284 22.1793 0.365182 21.728 0.121704 21.0973L12.2782 15.0273Z" fill="#012169" />
              <path d="M12.2782 7.19248V0H1.7333C1.00286 0.0115728 0.370979 0.474484 0.121704 1.12256L12.2782 7.19248Z" fill="#012169" />
              <path d="M0 7.40662H4.43478L0 5.19043V7.40662Z" fill="#012169" />
              <path d="M32 7.40647H27.542L32 5.17871V7.40647Z" fill="#012169" />
              <path d="M32 14.813H27.542L32 17.0407V14.813Z" fill="#012169" />
              <path d="M0 14.813H4.43478L0 17.0292V14.813Z" fill="#012169" />
              <path d="M32 1.88037L20.9565 7.40637H23.4261L32 3.12444V1.88037Z" fill="#C8102E" />
              <path d="M11.0203 14.813H8.55072L0 19.0833V20.3274L11.0435 14.813H11.0203Z" fill="#C8102E" />
              <path d="M6.09855 7.4123H8.56812L0 3.13037V4.36866L6.09855 7.4123Z" fill="#C8102E" />
              <path d="M25.8724 14.8076H23.4028L31.9999 19.1069V17.8686L25.8724 14.8076Z" fill="#C8102E" />
            </svg>
            <span>DLC</span>
          </div>
          <div className={css.tagItemValue}>
            <input
              type={'text'}
              defaultValue={tagDlcEn}
              value={tagDlcEn}
              placeholder={'DLC'}
              onChange={(event) => handleChange(setTagDlcEn, event)}
            />
          </div>
        </div>
      </div>
      <div className={css.tagTypeEditButtons}>
        <Button width={'217px'} height={'auto'} onClick={onSave}>Сохранить</Button>
        <Button width={'217px'} height={'auto'} onClick={onClose}>Закрыть</Button>
      </div>
      
    </ModalBase>);
}

export default ModalTypeTagEdit;