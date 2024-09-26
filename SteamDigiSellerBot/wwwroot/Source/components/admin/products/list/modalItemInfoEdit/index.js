import React, { useState, useEffect } from 'react';
import Button from '../../../../shared/button';
import DialogActions from '@mui/material/DialogActions';
import ModalBase from '../../../../shared/modalBase';
import Switch from '../../../../shared/switch';
import css from './styles.scss';
import {
  state,
  itemInfoTemplatesLoading,
  apiFetchItemInfoTemplateValues,
  apiCreateItemInfoTemplate,
  apiDeleteItemInfoTemplate,
  apiFetchTagTypeReplacementValues,
  apiFetchTagPromoReplacementValues
} from '../../../../../containers/admin/state';
import ModalTagsView from './ModalTagsView'
import Textarea from '../../../../shared/textarea'

const ModalItemInfoEdit = ({ isOpen, onSave, onCancel, viewMode, itemInfoTemplates }) => {
  if (!isOpen)
    return;

  const maxRussianTextLength = 3969;
  const maxEnglishTextLength = 3988;

  const languageCodes = ['ru-RU', 'en-US'];
  const buttonWidth = '386px';
  const buttonTemplateWidth = '172px';
  const buttonCreateTemplateWidth = '207px';
  const buttonTemplateHeight = '50px';
  const maxTemplatesCount = 100;
  const [russianText, setRussianText] = useState('');
  const [englishText, setEnglishText] = useState('');
  const [errorRussianText, setErrorRussianText] = useState('');
  const [errorEnglishText, setErrorEnglishText] = useState('');
  const [showTagsModal, setShowTagsModal] = useState(false);
  const [tagPromoReplacementValues, setTagPromoReplacementValues] = useState([])
  const [tagTypeReplacementValues, setTagTypeReplacementValues] = useState([])

  useEffect(() => {
    apiFetchTagPromoReplacementValues().then((data) => {
      if (!data || data.length == 0) {
        return;
      }

      setTagPromoReplacementValues(data.flatMap(x => x.tagPromoReplacementValues));
    })
  }, [])

  useEffect(() => {
    apiFetchTagTypeReplacementValues().then((data) => {
      if (!data || data.length == 0) {
        return;
      }

      setTagTypeReplacementValues(data.flatMap(x => x.tagTypeReplacementValues));
    })
  }, [])

  const getTitle = () => {
    if (viewMode === 'main') {
      return 'Настроить описание товара';
    } else if (viewMode === 'additional') {
      return 'Настроить дополнительную информацию о товаре';
    }
  }

  const handleChangeRussianText = (text) => {
    setRussianText(text);
  };

  const handleChangeEnglishText = (text) => {
    setEnglishText(text);
  };

  const handleDeleteTemplate = (itemInfoTemplate) => {
    apiDeleteItemInfoTemplate(itemInfoTemplate.id);
  }

  const onLoadTemplate = (itemInfoTemplate) => {
    apiFetchItemInfoTemplateValues(itemInfoTemplate.id).then((itemInfoTemplateValues) => {
      if (itemInfoTemplateValues && itemInfoTemplateValues.length === 2) {
        setRussianText(itemInfoTemplateValues[0].value);
        setEnglishText(itemInfoTemplateValues[1].value);
      }
    })
  };

  const onCreateTemplate = () => {
    let itemInfoTemplateValues = [
    {
        LanguageCode: languageCodes[0],
        Value: russianText,
    },
    {
      LanguageCode: languageCodes[1],
      Value: englishText,
    }]
    apiCreateItemInfoTemplate(itemInfoTemplateValues);
  };

  const getButtonsTemplate = () => {
    if (!itemInfoTemplates) {
      return;
    }

    var templates = itemInfoTemplates;
    return templates.map((val, index) => (
      <div className={css.loadTemplate}>
        <Button
          text={`Шаблон №${index+1}`}
          width={buttonTemplateWidth}
          height={buttonTemplateHeight}
          onClick={() => {
            onLoadTemplate(val);
          }}
          isDisabled={itemInfoTemplatesLoading}
          innerTextMargin={'0 16px 0 0'}
        />
        <svg width="15" height="14" viewBox="0 0 15 14" fill="none" xmlns="http://www.w3.org/2000/svg" onClick={() => handleDeleteTemplate(val)}>
          <path d="M10.8227 14H4.35081C3.03247 14 1.99377 13.055 1.99377 11.935V5.6C1.99377 5.39 2.15357 5.25 2.39327 5.25C2.63297 5.25 2.79277 5.39 2.79277 5.6V11.935C2.79277 12.705 3.51186 13.3 4.35081 13.3H10.8227C11.7016 13.3 12.3807 12.67 12.3807 11.935V5.6C12.3807 5.39 12.5405 5.25 12.7802 5.25C13.0199 5.25 13.1797 5.39 13.1797 5.6V11.935C13.1797 13.055 12.101 14 10.8227 14Z" />
          <path d="M12.9407 1.715H9.90447C9.70473 0.735 8.74593 0 7.58739 0C6.42885 0 5.47006 0.735 5.27031 1.715H2.23413C1.31529 1.715 0.596191 2.345 0.596191 3.15C0.596191 3.955 1.31529 4.55 2.23413 4.55H12.9806C13.8994 4.55 14.6185 3.92 14.6185 3.115C14.6185 2.31 13.8595 1.715 12.9407 1.715ZM7.58739 0.7C8.30648 0.7 8.90573 1.12 9.06553 1.715H6.0693C6.26905 1.12 6.8683 0.7 7.58739 0.7ZM12.9407 3.85H2.23413C1.79468 3.85 1.39519 3.535 1.39519 3.115C1.39519 2.73 1.75473 2.38 2.23413 2.38H12.9806C13.42 2.38 13.8195 2.695 13.8195 3.115C13.7796 3.535 13.42 3.85 12.9407 3.85Z" />
          <path d="M4.78976 12.2852C4.55006 12.2852 4.39026 12.1452 4.39026 11.9352V6.09023C4.39026 5.88023 4.55006 5.74023 4.78976 5.74023C5.02945 5.74023 5.18925 5.88023 5.18925 6.09023V11.9352C5.18925 12.1102 4.9895 12.2852 4.78976 12.2852Z" />
          <path d="M10.3836 12.2852C10.1439 12.2852 9.98413 12.1452 9.98413 11.9352V6.09023C9.98413 5.88023 10.1439 5.74023 10.3836 5.74023C10.6233 5.74023 10.7831 5.88023 10.7831 6.09023V11.9352C10.7831 12.1102 10.5834 12.2852 10.3836 12.2852Z" />
          <path d="M7.58822 12.2852C7.34852 12.2852 7.18872 12.1452 7.18872 11.9352V6.09023C7.18872 5.88023 7.34852 5.74023 7.58822 5.74023C7.82792 5.74023 7.98771 5.88023 7.98771 6.09023V11.9352C7.98771 12.1102 7.78797 12.2852 7.58822 12.2852Z" />
        </svg>
      </div>));
  }

  const getCreateTemplateButton = () => {
    if (!itemInfoTemplates || itemInfoTemplates.length >= maxTemplatesCount) {
      return;
    }

    return (<Button
      text={`Новый шаблон`}
      width={buttonCreateTemplateWidth}
      height={buttonTemplateHeight}
      onClick={() => {
        onCreateTemplate();
      }}
      isDisabled={itemInfoTemplatesLoading}
      className={css.btnCreateTemplate}/>);
  }

  const renderTitleBtn = () => {
    return (<Button
      width={`auto`}
      minWidth={`auto`}
      height={`auto`}
      className={css.titleButton}
      text={`Теги`}
      onClick={() => { setShowTagsModal(!showTagsModal) }}
    />)
  }

  const renderModals = () => {
    if (showTagsModal) {
      return (<ModalTagsView
          isOpen={showTagsModal}
          onClose={() => setShowTagsModal(!showTagsModal)}/>);
    }
  }

  const validateText = (russianText, englishText) => {    
    var russianTextLength = russianText.length
      + getTextActualLengthDiff(russianText, tagTypeReplacementValues, /%type%/g, "%type%", languageCodes[0])
      + getTextActualLengthDiff(russianText, tagPromoReplacementValues, /%promo%/g, "%promo%", languageCodes[0]);

    if (russianTextLength > maxRussianTextLength) {
      setErrorRussianText(`Превышена максимально возможная длина текста описания на русском языке (${maxRussianTextLength} символов)`)
      return false;
    } else {
      setErrorRussianText('')
    }

    var englishTextLength = englishText.length
      + getTextActualLengthDiff(englishText, tagTypeReplacementValues, /%type%/g, "%type%", languageCodes[1])
      + getTextActualLengthDiff(englishText, tagPromoReplacementValues, /%promo%/g, "%promo%", languageCodes[1]);

    if (englishTextLength > maxEnglishTextLength) {
      setErrorEnglishText(`Превышена максимально возможная длина текста описания на английском языке (${maxEnglishTextLength} символов)`)
      return false;
    } else {
      setErrorEnglishText('')
    }

    return true;
  }

  const getTextActualLengthDiff = (source, tagReplacementValues, patternTagValue, tagValue, languageCode) => {
    let tagCount = ((source || '').match(patternTagValue) || []).length;
    if (tagCount > 0) {
      let findedTagValue = tagReplacementValues.find(x => x.languageCode == languageCode);
      let diffTagReplacementValuesLength = findedTagValue.value.length - tagValue.length;
      return diffTagReplacementValuesLength * tagCount;
    }

    return 0;
  }

  const renderRussianErrors = () => {
    if (!errorRussianText) {
      return;
    }

    return (<div className={css.error}>{errorRussianText}</div>)
  }

  const renderEnglishErrors = () => {
    if (!errorEnglishText) {
      return;
    }

    return (<div className={css.error}>{errorEnglishText}</div>)
  }

  return (
    <div>
    <ModalBase
      isOpen={isOpen}
      title={getTitle()}
      renderTitleBtn={renderTitleBtn}
      width={1548}
      height={836}
      letterSpacing={'0.03em'}>
      <div className={css.wrapper}>
        <div className={css.textareaWrapper}>
          <div className={css.textareaContent}>
            <svg width="33" height="23" viewBox="0 0 33 23" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M1.77223 0H30.5082C31.4849 0 32.2804 0.793988 32.2804 1.76896V11.2092H0V1.76896C0 0.793988 0.795458 0 1.77223 0Z" fill="white" />
              <path d="M0 11.2095H32.2862V20.6498C32.2862 21.6247 31.4908 22.4187 30.514 22.4187H1.77223C0.795458 22.4187 0 21.6189 0 20.6498V11.2095Z" fill="#D52B1E" />
              <path d="M0 7.47266H32.2862V14.9455H0V7.47266Z" fill="#0039A6" />
              </svg>
            <Textarea
              onChange={handleChangeRussianText}
              defaultValue={russianText}
              value={russianText}
              placeholder={'Введенный текст с описанием товара'}
              height={'492px'}
              width={'100%'}
              />
              {renderRussianErrors()}
          </div>

          <div className={css.textareaContent}>
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
            <Textarea
              onChange={handleChangeEnglishText}
              defaultValue={englishText}
              value={englishText}
              placeholder={'Введенный текст с описанием товара'}
              height={'492px'}
              width={'100%'}
              />
              {renderEnglishErrors()}
          </div>
        </div>
        <div className={css.templates}>
          {getButtonsTemplate()}
          {getCreateTemplateButton()}
        </div>
        <div className={css.actions}>
          <Button
            text={'Сохранить'}
            style={{
              backgroundColor: '#478C35',
              marginRight: '46px'
            }}
            width={buttonWidth}
            onClick={() => {
              if (!validateText(russianText, englishText)) {
                return;
              }
              onSave(russianText, englishText);
              setRussianText('');
              setEnglishText('');
            }}
            isDisabled={itemInfoTemplatesLoading}
          />
          <Button
            text={'Назад'}
            onClick={async () => {
              if (onCancel) {
                setRussianText('');
                setEnglishText('');
                onCancel();
              }
            }}
            width={buttonWidth}
            style={{ backgroundColor: '#9A7AA9', marginLeft: '0px' }}
            isDisabled={itemInfoTemplatesLoading}
          />
        </div>
      </div>
      </ModalBase>
      {renderModals()}
    </div>);
}

export default ModalItemInfoEdit;