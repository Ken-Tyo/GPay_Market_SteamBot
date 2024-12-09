import React, { useEffect, useState, useRef } from "react";
import moment from "moment";
import CheckBox from "./listCheckBox";
import Section from "../../section";
import IconButton from "../../../shared/iconButton";
import Button from "../../../shared/button";
import Switch from "../../../shared/switch";
import css from "./styles.scss";
import trash from "../../../../icons/trash.svg";
import pen from "../../../../icons/pen.svg";
import addItem from "../../../../icons/additem.svg";
import warning from "../../../../icons/warning.svg";
import infinity from "../../../../icons/infinity.svg";
import {
  state,
  apiGetItem,
  apiSetItemActiveStatus,
  apiDeleteItem,
  apiBulkDeleteItem,
  apiChangeItem,
  apiCreateItem,
  apiUpdateItemInfoes,
  toggleBulkEditPercentModal,
  toggleBulkEditPriceBasisModal,
  toggleEditItemModal,
  toggleItemMainInfoModal,
  toggleItemAdditionalInfoModal,
  apiChangeItemBulk,
  apiChangePriceBasisBulk,
  setSelectedItem,
  setSelectedItems,
  setStateProp,
} from "../../../../containers/admin/state";
import ConfirmDialog from "../../../shared/modalConfirm";
import EditItemModal from "./modalEdit";
import EditItemInfoModal from "./modalItemInfoEdit";
import BulkPercentEdit from "./modalBulkPercentEdit";
import BulkPriceBasisEdit from "./modalBulkPriceBasisEdit";
import ToggleSort from "./modalSort/index";
import Popover from "@mui/material/Popover";
import Typography from "@mui/material/Typography";
import List from "../../../shared/list";
import { itemsMode } from "../../../../containers/admin/common";
import { CircularProgress } from "@mui/material";
const products = () => {
  const {
    items,
    itemsLoading,
    bulkEditPercentModalIsOpen,
    bulkEditPriceBasisModalIsOpen,
    editItemModalIsOpen,
    editItemMainInfoModalIsOpen,
    editItemAdditionalInfoModalIsOpen,
    selectedItem,
    selectedItems,
    currencies,
    itemsResponse,
    productsFilter,
    changeItemBulkResponse,
    itemInfoTemplates,
  } = state.use();

  const [sortedItems, setSortedItems] = useState([...items]);
  const [sortOrder, setSortOrder] = useState(null);
  const prntRef = useRef(null);

  const [openDelConfirm, setOpenDelConfirm] = useState(false);
  const [openMassDelConfirm, setOpenMassDelConfirm] = useState(false);

  const [errParsePriceText, setErrParsePriceText] = useState("");
  const [anchorEl, setAnchorEl] = React.useState(null);
  const [subMenuVisibility, setSubMenuVisibility] = useState(css.subMenuHidden);

  const [lastSelectedId, setLastSelectedId] = useState(-1);

  const open = Boolean(anchorEl);
  const massChangeButStyle = {
    width: "200px",
    height: "49px",
    backgroundColor: "#8A44AB",
    fontSize: "15px",
  };
  const INFINTITY_DATE = "9999-12-31T23:59:59.999999";
  const currencyDict = {};
  currencies.map((c) => {
    currencyDict[c.steamId] = c;
  });

    const handleSort = (type) => {
        let sorted = [...items];

        if (type === "price") {
            sorted.sort((a, b) => {
                if (sortOrder === "asc") {
                    if (a.fixedDigiSellerPrice !== null && b.fixedDigiSellerPrice !== null) {
                        return a.fixedDigiSellerPrice - b.fixedDigiSellerPrice;
                    } else if (a.fixedDigiSellerPrice !== null && b.fixedDigiSellerPrice === null) {
                        return 1;
                    } else if (a.fixedDigiSellerPrice === null && b.fixedDigiSellerPrice !== null) {
                        return -1;
                    }
                    return a.currentDigiSellerPrice - b.currentDigiSellerPrice;
                } else {
                    if (a.fixedDigiSellerPrice !== null && b.fixedDigiSellerPrice !== null) {
                        return b.fixedDigiSellerPrice - a.fixedDigiSellerPrice;
                    } else if (a.fixedDigiSellerPrice !== null && b.fixedDigiSellerPrice === null) {
                        return -1;
                    } else if (a.fixedDigiSellerPrice === null && b.fixedDigiSellerPrice !== null) {
                        return 1;
                    }
                    return b.currentDigiSellerPrice - a.currentDigiSellerPrice;
                }
            });
        } else if (type === 'percent') {
            sorted.sort((a, b) => {
                if (sortOrder === "asc") {
                    if (a.isFixedPrice && b.isFixedPrice) {
                        return a.currentDigiSellerPrice / a.currentSteamPriceRub - b.currentDigiSellerPrice / b.currentSteamPriceRub;
                    } else if (a.isFixedPrice && !b.isFixedPrice) {
                        return 1;
                    } else if (!a.isFixedPrice && b.isFixedPrice) {
                        return -1;
                    }
                    return a.steamPercent - b.steamPercent
                } else {
                    if (a.isFixedPrice && b.isFixedPrice) {
                        return b.currentDigiSellerPrice / b.currentSteamPriceRub - a.currentDigiSellerPrice / a.currentSteamPriceRub;
                    } else if (a.isFixedPrice && !b.isFixedPrice) {
                        return -1;
                    } else if (!a.isFixedPrice && b.isFixedPrice) {
                        return 1;
                    }
                    return b.steamPercent - a.steamPercent
                }
            });
        } else if(type === 'discountPercent') {
            const getDiscountValue = (discount) => {
              if (discount != null && discount > 0) {
                  return sortOrder === "asc" ? discount : -discount;
              }
              return Infinity;
            };
    
            sorted.sort((a, b) => {
              const aDiscount = getDiscountValue(a.discountPercent);
              const bDiscount = getDiscountValue(b.discountPercent);
              return aDiscount - bDiscount;
          });
        } else {
            sorted.sort((a, b) => {
                return a.id - b.id;
            });
          };
        setSortedItems(sorted);
        setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    };

    useEffect(() => {
        handleSort('id');
        }, [items]);


  const headers = {
    checkbox: <div style={{ maxWidth: "86px", minWidth: "86px" }}></div>,
    game: "Игра",
    product: (
      <div style={{ display: "flex", alignItems: "center" }}>
        <div>Товар</div>
        <img
          src={addItem}
          style={{ marginLeft: "10px", cursor: "pointer" }}
          onClick={async () => {
            await setSelectedItem({ steamCurrencyId: 5 });
            toggleEditItemModal(true);
          }}
        />
      </div>
    ),
    price: (
        <div style={{ display: "flex", alignItems: "center" }}>
            <div>Цена</div>
            <ToggleSort orderSort={sortOrder} onSort={handleSort} />
        </div>
    ),
    lastRegion: "",
    discount: "",
    options: "Опции",
    active: "",
  };

    // Get subarray of Ids of the sortedItems of elements between selected items with id-s: firstId, secondId
    const getItemsBetweenSelected = (firstId, secondId) => {
        const startIndex = sortedItems.findIndex(item => item.id === firstId);
        const endIndex = sortedItems.findIndex(item => item.id === secondId);

        if (startIndex === -1 || endIndex === -1) return [];

        if (startIndex == endIndex) return [sortedItems[startIndex]];

        const sliceStart = Math.min(startIndex, endIndex);
        const sliceEnd = Math.max(startIndex, endIndex);

        return getSliceOfSortedItems(sliceStart, sliceEnd);
    };

    //Shift on the unselected
    const addItemsToSelected = (shiftId) => {
        let startIndex = -1;
        let endIndex = -1;
        let curId = -1;
        let shiftIndex = sortedItems.findIndex(item => item.id === shiftId);

        if (selectedItems.length == 0) return [sortedItems[shiftIndex]];
                
        for (let i = 0; i < sortedItems.length; i++) {
            curId = sortedItems[i].id;
            if (selectedItems.includes(curId)) {
                if (startIndex < 0) startIndex = i;
                endIndex = i;
            }
        }
        if (startIndex < 0 || endIndex < 0 || shiftIndex < 0) return [];

        const sliceStart = Math.min(startIndex, endIndex, shiftIndex);
        const sliceEnd = Math.max(startIndex, endIndex, shiftIndex);

        return getSliceOfSortedItems(sliceStart, sliceEnd);
    };

    //Shift on the selected
    const removeItemsFromSelected = (shiftId) => {
        let startIndex = -1;
        let endIndex = -1;
        let curId = -1;
        let shiftIndex = sortedItems.findIndex(item => item.id === shiftId);

        if (selectedItems.length == 1) return [];

        for (let i = 0; i < sortedItems.length; i++) {
            curId = sortedItems[i].id;
            if (selectedItems.includes(curId)) {
                if (startIndex < 0) startIndex = i;
                endIndex = i;
            }
        }

        if (startIndex < 0 || endIndex < 0 || shiftIndex < 0) return [];
        let sliceStart;
        let sliceEnd;

        //Just to be sure that shiftIndex between start and end
        if (shiftIndex < startIndex || startIndex > endIndex) return [];
        if (shiftIndex == startIndex) {
            sliceStart = startIndex + 1;
            sliceEnd = endIndex;
        }
        else if (shiftIndex == endIndex) {
            sliceStart = startIndex;
            sliceEnd = endIndex - 1;
        }
        else {
            // Reduce top or botton of selected
            if (lastSelectedId < shiftIndex) {
                sliceStart = startIndex;
                sliceEnd = shiftIndex - 1;
            }
            else {
                sliceStart = shiftIndex + 1;
                sliceEnd = endIndex;
            }
        }

        return getSliceOfSortedItems(sliceStart, sliceEnd);
    };

    const getSliceOfSortedItems = (sliceStart, sliceEnd) => {
        let arrNew;

        if (sliceEnd >= (sortedItems.length - 1))
            arrNew = sortedItems.slice(sliceStart).map(item => item.id);
        else
            arrNew = sortedItems.slice(sliceStart, sliceEnd + 1).map(item => item.id);

        return arrNew; 
    }

  const getBtnMassDescriptionBlock = (lines) => {
    return <div className={css.massDescriptionText}>{lines.map(line => (<div>{line}</div>))}</div>;
  };

  const toggleMassDescriptionSubMenu = () => {
    if (subMenuVisibility == css.subMenuVisible) {
      setSubMenuVisibility(css.subMenuHidden);
    } else {
      setSubMenuVisibility(css.subMenuVisible);
    }
  }

  const getLoadingText = () => {
    if (changeItemBulkResponse.loading) {
      return "Происходит обновление цен";
    }
    if (changeItemBulkResponse.loadingItemInfo) {
      return "Происходит обновление описаний товаров";
    }
    return "Подгружаем товары";
  }

  return (
      <div className={css.wrapper} style={{ userSelect: 'none' }}>
      <List
        headers={Object.values(headers)}
        data={[...sortedItems]}
        isLoading={itemsLoading}
        loadingText={getLoadingText}
        itemRenderer={(i) => {
          let priceColor = "#D4D4D4";
          if (i.currentDigiSellerPrice > i.currentSteamPriceRub)
            priceColor = "#EDBE16";
          else if (i.currentDigiSellerPrice < i.currentSteamPriceRub)
            priceColor = "#13E428";

          let activeRow = "";
          if (selectedItems.indexOf(i.id) !== -1) activeRow = css.active;

          let discountEndTime = "";
          let discountEndTimeExpired = !i.isDiscount;
          if (i.isDiscount) {
            if (i.discountEndTime == INFINTITY_DATE) {
              discountEndTime = "∞";
            } else {
              var offset = new Date().getTimezoneOffset();

              let det = moment(i.discountEndTime).add(-1 * offset, "minutes");
              let last = moment.duration(det.diff(moment()));
              let hoursToShowCountDown = 24;
              if (last.asHours() > hoursToShowCountDown) {
                discountEndTime = "до " + det.format("DD.MM");
              } else if (
                last.asHours() > 0 &&
                last.asHours() <= hoursToShowCountDown
              ) {
                discountEndTime = `${last
                  .hours()
                  .toFixed(0)
                  .padStart(2, "0")}ч. ${(last.minutes() % 60)
                  .toFixed(0)
                  .padStart(2, "0")}м.`;
              } else {
                discountEndTimeExpired = true;
              }
            }
          }

          let steamPriceColor = discountEndTimeExpired ? "#D4D4D4" : "#CCCF1C";

          const getDiffPriceInPercent = () => {
            if (i.currentSteamPriceRub === 0) return 0;

            return (
              (i.currentDigiSellerPrice * 100) /
              i.currentSteamPriceRub
            ).toFixed(0);
          };
          let additionalInfo = i.isFixedPrice
            ? `${getDiffPriceInPercent()}%`
            : `${i.steamPercent}% ${i.addPrice} rub`;

          return (
            <tr key={i.id} className={activeRow}>
              <td>
                <div className={css.cell}>
                  <div className={css.listItemCheckbox}>
                    <CheckBox
                        onCheckedChange={(val, shiftPressed) => {
                        let arrNew;
                        if (shiftPressed) {
                            if (val) {
                                // Shift-mouse click after selected: Select all items betweeen the current row and the last selected
                                if (lastSelectedId > 0) {
                                    arrNew = getItemsBetweenSelected(lastSelectedId, i.id);
                                    setLastSelectedId(-1);
                                }
                                else {  //Add new portion  (2-nd shift selects new)
                                    arrNew = addItemsToSelected(i.id);
                                }
                            }
                            else { // Remove portion  (2-nd shift unselects old)
                                arrNew = removeItemsFromSelected(i.id);
                            }
                        }
                        else { 
                            let newLastId = -1;
                            if (val) {
                                arrNew = [...selectedItems, i.id];
                                newLastId = i.id;
                            } else {
                                arrNew =  selectedItems.filter((id) => id != i.id);
                                if (arrNew.length == 1) newLastId = newArr[0];
                            }
                            setLastSelectedId(newLastId);
                        }
                        setSelectedItems(arrNew);
                     }
                     }
                      value= {selectedItems.indexOf(i.id) !== -1}
                    />
                  </div>
                </div>
              </td>
              <td>
                <div className={css.cell}>
                  <div className={css.game}>
                    <div>{i.appId}</div>
                    <div>
                      ({i.subId}){" "}
                      {i.isDlc && <span className={css.dlc}>DLC</span>}
                    </div>
                  </div>
                </div>
              </td>
              <td>
                <div className={css.cell} style={{ justifyContent: "start" }}>
                  <div className={css.product}>
                    {/* <div>Полное название товара Digiseller</div> */}
                    <div>
                      <span
                        style={i.name === "Error" ? { color: "#A12C2C" } : {}}
                      >
                        {i.name}
                      </span>
                    </div>
                    <div>{i.digiSellerIds && i.digiSellerIds.join(",")}</div>
                  </div>
                </div>
              </td>
              <td>
                <div className={css.cell}>
                  <div className={css.price}>
                    {!i.isPriceParseError && (
                      <div className={css.items}>
                        <div
                          style={{ cursor: "pointer" }}
                          onClick={async () => {
                            let item = await apiGetItem(i.id);
                            setSelectedItem(item);
                            setStateProp("itemsMode", itemsMode[2]);
                          }}
                        >
                          <span style={{ color: priceColor }}>
                            {i.currentDigiSellerPrice.toFixed(0)} rub
                          </span>{" "}
                          |{" "}
                          <span>
                            {i.currentSteamPrice}{" "}
                            {currencyDict[i.steamCurrencyId].steamSymbol}
                          </span>
                        </div>
                        <div style={{ color: steamPriceColor }}>
                          {additionalInfo}
                        </div>
                      </div>
                    )}
                    {i.isPriceParseError && (
                      <div className={css.priceParseErr}>
                        <div className={css.errMes}>
                          <div>Ошибка</div>
                          <Typography
                            aria-owns={open ? "mouse-over-popover" : undefined}
                            aria-haspopup="true"
                            onMouseEnter={(event) => {
                              setAnchorEl(event.currentTarget);
                              if (i.currentSteamPriceRub < 0) {
                                setErrParsePriceText(
                                  "Возможно проблема связана с парсингом цены и валютой."
                                );
                                return;
                              }
                              i.isBundle
                                ? setErrParsePriceText(
                                    "Добавьте хотя-бы одного бота с нужным регионом под парсинг. Без этого собрать цену бандла невозможно"
                                  )
                                : setErrParsePriceText(
                                    "Добавьте хотя-бы одного бота с прокси, который НЕ относится к РФ региону для парсинга цены"
                                  );
                            }}
                            onMouseLeave={() => {
                              setAnchorEl(null);
                            }}
                          >
                            <img src={warning} />
                          </Typography>
                        </div>
                        <div className={css.errState}>
                          95% - <span style={{ color: "#A12C2C" }}>ОШИБКА</span>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </td>
              <td>
                <div className={css.cell}>
                  {i.lastSendedRegionCode && (
                    <div className={css.lastSendedRegion}>
                      {i.lastSendedRegionCode}
                    </div>
                  )}
                </div>
              </td>
              <td>
                <div className={css.cell}>
                  {i.isDiscount && !discountEndTimeExpired && (
                    <div className={css.discount}>
                      <Section
                        className={css.badge}
                        bgcolor={"#A9AC26"}
                        height={39}
                        width={116}
                      >
                        <div className={css.text}>-{i.discountPercent}%</div>
                      </Section>
                      <div className={css.date}>
                        {discountEndTime == "∞" ? (
                          <img src={infinity} />
                        ) : (
                          discountEndTime
                        )}
                      </div>
                    </div>
                  )}
                </div>
              </td>
              <td>
                <div className={css.cell}>
                  <div className={css.buttons}>
                    <div className={css.btnWrapper}>
                      <IconButton
                        icon={pen}
                        onClick={async () => {
                          await setSelectedItem(i);
                          toggleEditItemModal(true);
                        }}
                      />
                    </div>
                    <div className={css.btnWrapper}>
                      <IconButton
                        icon={trash}
                        onClick={() => {
                          setSelectedItem(i);
                          setOpenDelConfirm(!openDelConfirm);
                        }}
                      />
                    </div>
                  </div>
                </div>
              </td>
              <td>
                <div className={css.cell}>
                  <div className={css.buttons}>
                    <div style={{ marginLeft: "30px", marginRight: "15px" }}>
                      <Switch
                        value={i.active}
                        onChange={() => {
                          apiSetItemActiveStatus([i.id]);
                        }}
                      />
                    </div>
                  </div>
                </div>
              </td>
            </tr>
          );
        }}
      />

      <div
        className={
          css.massChangeMenu +
          " " +
          (selectedItems.length > 0 ? css.active : "")
        }
      >
        <div className={css.title}>Выделено: {selectedItems.length}</div>
        <div className={css.actions}>
          <div className={css.btnWrapper}>
            <Button
              text={
                selectedItems.length === items.length
                  ? "Снять выделения"
                  : "Выделить все"
              }
              style={massChangeButStyle}
              onClick={() => {
                if (selectedItems.length === items.length) {
                  setSelectedItems([]);
                } else {
                  setSelectedItems(items.map((i) => i.id));
                }
              }}
            />
          </div>
          <div className={css.btnWrapper}>
            <Button
              text={"Вкл/выкл группу"}
              style={massChangeButStyle}
              onClick={() => {
                apiSetItemActiveStatus(selectedItems);
              }}
            />
          </div>
          <div className={css.btnWrapper}>
            <Button
              text={"Массовая смена цен"}
              style={massChangeButStyle}
              onClick={() => {
                toggleBulkEditPercentModal(true);
              }}
            />
          </div>
          <div className={css.btnWrapper}>
            <Button
              text={getBtnMassDescriptionBlock(['Смена описания/', 'доп. информации'])}
              style={massChangeButStyle}
              onClick={() => {
                toggleMassDescriptionSubMenu();
              }}
            />

            <div className={css.subMenu + ' ' + css.massDescriptionBlockSubMenu + ' ' + subMenuVisibility}>
              <div className={css.subMenuItem} onClick={() => {
                toggleItemMainInfoModal(true);
                toggleMassDescriptionSubMenu();
              }
              }>Основная информация</div>
              <div className={css.subMenuItem} onClick={() => {
                toggleItemAdditionalInfoModal(true);
                toggleMassDescriptionSubMenu();
              }
              }>Доп. информация</div>
            </div>
            </div>
            <div className={css.btnWrapper}>
                <Button
                    text={"Смена ценовой основы"}
                    style={massChangeButStyle}
                    onClick={() => {
                        toggleBulkEditPriceBasisModal(true);
                    }}
                />
            </div>
            <div className={css.btnWrapper}>
                <Button
                    text={"Удалить все"}
                    style={massChangeButStyle}
                    onClick={() => {
                        setOpenMassDelConfirm(true);
                    }}
                />
            </div>
        </div>
      </div>

      <EditItemModal
        isOpen={editItemModalIsOpen}
        value={selectedItem}
        onCancel={() => {
          toggleEditItemModal(false);
        }}
        onSave={(newItem) => {
          if (newItem.id) apiChangeItem(newItem);
          else apiCreateItem(newItem);
        }}
      />
      <EditItemInfoModal
        isOpen={editItemMainInfoModalIsOpen || editItemAdditionalInfoModalIsOpen}
        viewMode={editItemMainInfoModalIsOpen ? 'main' : (editItemAdditionalInfoModalIsOpen ? 'additional' : 'none')}
        onCancel={() => {
          toggleItemMainInfoModal(false);
          toggleItemAdditionalInfoModal(false);
        }}
        onSave={(russianText, englishText) => {
          var goods = items
            .filter(i => selectedItems.includes(i.id))
            .map(x => {
              return {
                digiSellerIds: x.digiSellerIds,
                itemId: x.id,
              };
            });

          var updateItemInfoesCommands = {
            description: editItemMainInfoModalIsOpen ? [{
              locale: "ru-RU",
              value: russianText
            }, {
              locale: "en-US",
              value: englishText
            }] : [],
            add_info: !editItemMainInfoModalIsOpen ? [{
              locale: "ru-RU",
              value: russianText
            }, {
              locale: "en-US",
              value: englishText
            }] : [],
            goods: goods
          }

          toggleItemMainInfoModal(false);
          toggleItemAdditionalInfoModal(false);
          apiUpdateItemInfoes(updateItemInfoesCommands);
        }}
        itemInfoTemplates={ itemInfoTemplates }
      />
      <ConfirmDialog
        title={"Подтвердите удаление"}
        content={`Вы действительно хотите удалить ${selectedItems.length} выделенных позиций?`}
        isOpen={openMassDelConfirm}
        onConfirm={{
          action: () => {
            apiBulkDeleteItem(selectedItems);
            setSelectedItems([]);
            setOpenMassDelConfirm(false);
          },
        }}
        onCancel={{
          action: () => {
            setOpenMassDelConfirm(false);
          },
        }}
      />
      <ConfirmDialog
        title={"Подтвердите удаление"}
        content={selectedItem && selectedItem.name}
        isOpen={openDelConfirm}
        onConfirm={{
          action: () => {
            apiDeleteItem(selectedItem.id);
            setSelectedItems(
              selectedItems.filter((id) => id != selectedItem.id)
            );
            setOpenDelConfirm(false);
          },
        }}
        onCancel={{
          action: () => {
            setOpenDelConfirm(false);
          },
        }}
      />
      <BulkPercentEdit
        isOpen={bulkEditPercentModalIsOpen}
        onCancel={() => {
          toggleBulkEditPercentModal(false);
        }}
        onSave={(val, increaseDecreaseOperator, increaseDecreaseVal) => {
          toggleBulkEditPercentModal(false);
          apiChangeItemBulk(val, increaseDecreaseOperator.id, increaseDecreaseVal, selectedItems);
        }}
      />
      <BulkPriceBasisEdit
          isOpen={bulkEditPriceBasisModalIsOpen}
          onCancel={() => {
              toggleBulkEditPriceBasisModal(false);
          }}
          onSave={(val) => {
              toggleBulkEditPriceBasisModal(false);
              apiChangePriceBasisBulk(val, selectedItems);
          }}
          selectedCount={selectedItems.length}
      />
      <Popover
        id={`mouse-over-popover`}
        sx={{
          pointerEvents: "none",
        }}
        open={open}
        anchorEl={anchorEl}
        anchorOrigin={{
          vertical: "bottom",
          horizontal: "left",
        }}
        transformOrigin={{
          vertical: "top",
          horizontal: "left",
        }}
        onClose={() => {
          setAnchorEl(null);
        }}
        disableRestoreFocus
      >
        <Typography
          sx={{
            width: "327px",
            //height: '50px',
            color: "#D4D4D4",
            backgroundColor: "#43294B",

            padding: "14px 21px 13px 18px",
            fontSize: "16px",
            lineHeight: "20px",
            borderRadius: "none",
          }}
        >
          <div>{errParsePriceText}</div>
        </Typography>
      </Popover>
    </div>
  );
};

export default products;
