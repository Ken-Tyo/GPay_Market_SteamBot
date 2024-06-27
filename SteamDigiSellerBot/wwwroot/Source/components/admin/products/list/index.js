import React, { useEffect, useState } from "react";
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
  toggleBulkEditPercentModal,
  toggleEditItemModal,
  apiChangeItemBulk,
  setSelectedItem,
  setSelectedItems,
  setStateProp,
} from "../../../../containers/admin/state";
import ConfirmDialog from "../../../shared/modalConfirm";
import EditItemModal from "./modalEdit";
import BulkPercentEdit from "./modalBulkPercentEdit";
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
    editItemModalIsOpen,
    selectedItem,
    selectedItems,
    currencies,
    itemsResponse,
    productsFilter,
    changeItemBulkResponse,
  } = state.use();

  const [openDelConfirm, setOpenDelConfirm] = useState(false);
  const [openMassDelConfirm, setOpenMassDelConfirm] = useState(false);

  const [errParsePriceText, setErrParsePriceText] = useState("");
  const [anchorEl, setAnchorEl] = React.useState(null);
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
    price: "Цена",
    lastRegion: "",
    discount: "",
    options: "Опции",
    active: "",
  };
  console.log(itemsLoading);
  return (
    <div className={css.wrapper}>
      <List
        headers={Object.values(headers)}
        data={[...items]}
        isLoading={itemsLoading}
        loadingText={() => {
          if (changeItemBulkResponse.loading) {
            return "Происходит обновление цен";
          }
          return "Подгружаем товары";
        }}
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
                      onClick={(val) => {
                        if (val) {
                          let newArr = [...selectedItems];
                          newArr.push(i.id);
                          setSelectedItems(newArr);
                        } else {
                          let newArr = selectedItems.filter((id) => id != i.id);
                          setSelectedItems(newArr);
                        }
                      }}
                      value={selectedItems.indexOf(i.id) !== -1}
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
          console.log(newItem);
          if (newItem.id) apiChangeItem(newItem);
          else apiCreateItem(newItem);
        }}
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
        onSave={(val) => {
          toggleBulkEditPercentModal(false);
          apiChangeItemBulk(val, selectedItems);
        }}
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
