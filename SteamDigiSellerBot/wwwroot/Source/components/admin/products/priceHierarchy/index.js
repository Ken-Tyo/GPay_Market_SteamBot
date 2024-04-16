import React, { useState } from 'react';
import css from './styles.scss';
import {
  state,
  setStateProp,
  setItemPrice,
  setItemPricePriority,
} from '../../../../containers/admin/state';
import { itemsMode as mode } from '../../../../containers/admin/common';
import IconButton from '../../../shared/iconButton';
import leftImg from './left.svg';
import warning from './warning.svg';
import pen10x10 from '../../../../icons/pen10x10.svg';
import { TransformWrapper, TransformComponent } from 'react-zoom-pan-pinch';
import ModalSetItemPriceManual from './modalSetPriceManual';
const priceHierarchy = () => {
  const { selectedItem } = state.use();

  let prices = selectedItem?.priceHierarchy;
  let percentDiff = selectedItem?.percentDiff;
  //console.log(prices);
  const [selectedPrice, setSelectedPrice] = useState({});
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div className={css.priceHierarchyWrapper}>
      <div className={css.header}>
        <div className={css.left}>
          <div className={css.backBut}>
            <IconButton
              icon={leftImg}
              className={css.but}
              onClick={() => {
                setStateProp('itemsMode', mode[1]);
              }}
            />
          </div>
          <div className={css.title}>
            Ценовая иерархия -
            <div className={css.gameName}>{selectedItem?.name}</div>
          </div>
        </div>
        <div className={css.right}>Настроить правила</div>
      </div>
      <div className={css.scrollableDiv}>
      <div className={css.content}>
        <TransformWrapper
          initialScale={1}
          minScale={0.6}
          limitToBounds={false}
          doubleClick={{ disabled: true }}
          panning={{ velocityDisabled: true, activationKeys: ['Control'] }}
          wheel={{ step: 0.05 }}
        >
          <TransformComponent
            contentStyle={{ height: '100%', width: '100%' }}
            wrapperStyle={{ height: '100%', width: '100%' }}
          >
            <div className={css.hierarchy}>
              {Object.keys(prices || {}).map((levNum, idx) => {
                let items = prices[levNum];
                return (
                  <div className={css.level}>
                    <div className={css.prices}>
                      {items.map((i, itemIdx) => {
                        let dotColor = '#B1A9A9';
                        let nameColor = '#fff';
                        if (i.isPriority) {
                          nameColor = dotColor = '#77C863';
                          if (i.isNotBotExists) {
                            nameColor = dotColor = '#C6C93F';
                          }
                          if (i.failUsingCount >= 3) {
                            nameColor = dotColor = '#C82F2F';
                          }
                        }

                        return (
                          <div className={css.priceItemWrapper}>
                            <div className={css.priceItem}>
                              <div
                                className={css.nameRow}
                                onClick={(e) => {
                                  if (!e.ctrlKey) {
                                    setItemPricePriority(
                                      i.id,
                                      selectedItem?.id
                                    );
                                  }
                                }}
                              >
                                <div
                                  style={{ backgroundColor: dotColor }}
                                  className={css.dot}
                                ></div>
                                <div
                                  style={{ color: nameColor }}
                                  className={css.currName}
                                >
                                  {i.currencyName}
                                </div>
                              </div>
                              <div className={css.priceRow}>
                                {i.isManualSet && (
                                  <div className={css.warn}>
                                    <img src={warning} />
                                  </div>
                                )}
                                <div className={css.price}>
                                  {i.price} - {i.priceRub}
                                </div>
                                <div
                                  className={css.editBut}
                                  onClick={(e) => {
                                    if (!e.ctrlKey) {
                                      setSelectedPrice(i);
                                      setIsOpen(true);
                                      //console.log('click');
                                    }
                                  }}
                                >
                                  <img src={pen10x10} />
                                </div>
                              </div>
                            </div>
                            {itemIdx !== items.length - 1 && (
                              <div className={css.percent}>
                                <div>({percentDiff[i.id].toFixed(1)}%)</div>
                                <div className={css.line}></div>
                              </div>
                            )}
                          </div>
                        );
                      })}
                    </div>
                    {idx !== Object.keys(prices || {}).length - 1 && (
                      <div className={css.lineWrapper}>
                        <div className={css.percent}></div>
                        <div className={css.line}></div>
                        <div className={css.percent}>
                          ({percentDiff[items[items.length - 1].id].toFixed(1)}
                          %)
                        </div>
                      </div>
                    )}
                  </div>
                );
              })}
            </div>
          </TransformComponent>
        </TransformWrapper>
        </div>
      </div>

      <ModalSetItemPriceManual
        isOpen={isOpen}
        value={selectedPrice}
        onCancel={() => {
          setIsOpen(false);
        }}
        onSave={(val) => {
          setItemPrice(selectedPrice.id, val, selectedItem?.id);
          setIsOpen(false);
        }}
      />
    </div>
  );
};

export default priceHierarchy;
