import React, { useState, useEffect } from "react";
import Button from "../../../../shared/button";
import ModalBase from "../../../../shared/modalBase";
import css from "./styles.scss";
import FormItemText from "../../../../shared/formItem/text";
import FormItemSelect from "../../../../shared/formItem/select";
import { state } from "../../../../../containers/admin/state";
import TextBox from "../../../../shared/textbox2";
import Select from "../../../../shared/select";

import OutlinedInput from "@mui/material/OutlinedInput";
import InputLabel from "@mui/material/InputLabel";
import MenuItem from "@mui/material/MenuItem";
import FormControl from "@mui/material/FormControl";
import ListItemText from "@mui/material/ListItemText";
import MUISelect from "@mui/material/Select";
import Checkbox from "@mui/material/Checkbox";
import StyledOption from "../../../../shared/select/styledOption";
import MultiSelect from "../../../../shared/multiselect/index";
import MultipleSelectCheckmarks from "../../../../shared/formItem/select/checkmarksSelect";

import classNames from "classnames";
import FillCheckBox from "../../../../shared/checkBox";
import SymbolTextBox from "../../../../shared/SymbolTextbox";

// const ITEM_HEIGHT = 48;
// const ITEM_PADDING_TOP = 8;
// const MenuProps = {
//   PaperProps: {
//     style: {
//       maxHeight: ITEM_HEIGHT * 4.5 + ITEM_PADDING_TOP,
//       width: 250,
//     },
//   },
// };

const ModalFilter = ({ isOpen, value, onCancel, onSave }) => {
  const initial = {
    appId: "",
    productName: "",
    steamCurrencyId: [],
    gameRegionsCurrency: [],
    steamCountryCodeId: "",
    digiSellerIds: "",
    thirdPartyPriceValue: null,
    thirdPartyPriceType: false,
    hierarchyParams_targetSteamCurrencyId: null,
    hierarchyParams_baseSteamCurrencyId: null,
    hierarchyParams_compareSign: "<>",
    hierarchyParams_percentDiff: null,
    hierarchyParams_isActiveHierarchyOn: true,
  };
  const [item, setItem] = useState(initial);
  const { digiPriceSetType } = state.use();
  const signOptions = [
    { id: 0, name: ">=" },
    { id: 1, name: "=<" },
    { id: 2, name: "<>" },
  ];

  useEffect(() => {
    if (value) {
      let stateVal = {
        ...initial,
        ...value,
      };

      if (!stateVal.statusId) stateVal.statusId = 0;

      setItem(stateVal);
    }
  }, [value]);

  const currencies = state.use().currencies.map((c) => {
    return {
      id: c.steamId,
      name: c.code,
    };
  });

  const memoCurrencies = React.useMemo(() => currencies, []);

  const regions = state.use().steamRegions.map((c) => {
    return {
      id: c.id,
      name: c.name,
    };
  });

  const memoRegions = React.useMemo(() => regions, []);
  const digiSellerIdsIsNotValid = new RegExp("[^,\\d]");
  const handleChange = (prop) => (val, newVal) => {
    if (val == null) {
      return;
    }
    if (prop === "steamCurrencyId" || prop === "gameRegionsCurrency") {
      if (val != null) {
        //var newVal = val.targer.value;
        //var resultVal = newVal.map(e => currencies.find((c) => c.name === e).id);
        //debugger;
        val = val.target.value;
      } else {
        return;
      }
    } else if (prop === "digiSellerIds") {
      if (digiSellerIdsIsNotValid.test(val)) {
        return;
      }
    } else if (prop === "hierarchyParams_percentDiff") {
      var isValid = !Number.isNaN(val);
      if (!isValid) {
        return;
      }
    } else if (prop === "thirdPartyPriceValue") {
      var isValid = !Number.isNaN(val);
      if (!isValid) {
        return;
      }
    } else if (prop === "thirdPartyPriceType") {
      val = val === "₽";
    }
    console.log(item);
    console.log(prop, val);
    setItem({ ...item, [prop]: val });
  };
  const isNullOrEmpty = (target) => target != null && target != "";
  const handleOnSave = (transferObject) => {
    if (isNullOrEmpty(transferObject.hierarchyParams_baseSteamCurrencyId)) {
      transferObject.hierarchyParams_baseSteamCurrencyId = currencies.find(
        (c) => c.name === transferObject.hierarchyParams_baseSteamCurrencyId
      ).id;
    }
    if (isNullOrEmpty(transferObject.hierarchyParams_targetSteamCurrencyId)) {
      transferObject.hierarchyParams_targetSteamCurrencyId = currencies.find(
        (c) => c.name === transferObject.hierarchyParams_targetSteamCurrencyId
      ).id;
    }

    if (isNullOrEmpty(transferObject.steamCountryCodeId)) {
      transferObject.steamCountryCodeId = regionVal;
    }
    transferObject.IsFilterOn = true;
    onSave(transferObject);
  };

  const regionVal = (
    regions.find((c) => c.name === item.steamCountryCodeId) || {}
  ).id;

  const thirdPartyPriceTypeVal = item.thirdPartyPriceType
    ? digiPriceSetType[1].name
    : digiPriceSetType[0].name;

  return (
    <ModalBase
      isOpen={isOpen}
      title={"Фильтры отображения"}
      width={705}
      height={734}
    >
      <div className={css.content}>
        <FormItemText
          name={"AppID:"}
          onChange={handleChange("appId")}
          value={item.appId}
        />
        <FormItemText
          name={"Название товара:"}
          onChange={handleChange("productName")}
          value={item.itemName}
        />
        <div className={css.formItem}>
          <div className={css.name}>Регион получения:</div>

          <div className={css.wrapper}>
            <div className={css.doubleControl}>
              <Select
                options={regions}
                defaultValue={item.steamCountryCodeId}
                onChange={handleChange("steamCountryCodeId")}
                width={302}
              />
            </div>
          </div>
        </div>
        <div className={css.formItem}>
          <div className={css.name}>Ценовая основа:</div>

          <div className={css.wrapper}>
            <div>
              <MultipleSelectCheckmarks
                options={memoCurrencies}
                value={item.steamCurrencyId}
                onChange={handleChange("steamCurrencyId")}
              />
            </div>
          </div>
        </div>
        <div className={css.formItem}>
          <div className={css.name}>Регионы иерархии:</div>

          <div className={css.wrapper}>
            <div>
              <MultipleSelectCheckmarks
                options={memoCurrencies}
                value={item.gameRegionsCurrency}
                onChange={handleChange("gameRegionsCurrency")}
              />
            </div>
          </div>
        </div>
        <div className={css.formItem}>
          <div className={css.name}>Параметры:</div>

          <div className={css.wrapper}>
            <div className={css.doubleControl} style={{ gridGap: "9px" }}>
              <Select
                options={currencies}
                defaultValue={item.hierarchyParams_targetSteamCurrencyId}
                onChange={handleChange("hierarchyParams_targetSteamCurrencyId")}
                width={124}
              />
              <Select
                options={signOptions}
                defaultValue={item.hierarchyParams_compareSign}
                onChange={handleChange("hierarchyParams_compareSign")}
                width={68}
              />

              <TextBox
                onChange={handleChange("hierarchyParams_percentDiff")}
                defaultValue={item.hierarchyParams_percentDiff}
                width={92}
                type="number"
                cymbol={"%"}
              />
            </div>
          </div>
        </div>

        <div className={css.formItem}>
          <div className={css.name}></div>

          <div className={css.wrapper}>
            <div className={css.doubleControl}>
              <div style={{ fontSize: "18px", paddingLeft: "11px" }}>
                искать к основе:
              </div>
              <Select
                options={currencies}
                defaultValue={item.hierarchyParams_baseSteamCurrencyId}
                onChange={handleChange("hierarchyParams_baseSteamCurrencyId")}
                width={124}
              />
            </div>
          </div>
        </div>
        <div className={css.formItem}>
          <div className={css.name}></div>

          <div className={css.wrapper}>
            <div className={css.doubleControl} style={{ padding: "0 11px" }}>
              <div style={{ fontSize: "18px" }}>Включая активную иерархию:</div>
              <div className={css.wrapper} style={{ width: "30px" }}>
                <FillCheckBox
                  size={30}
                  checked={item.hierarchyParams_isActiveHierarchyOn}
                  onChange={handleChange("hierarchyParams_isActiveHierarchyOn")}
                />
              </div>
            </div>
          </div>
        </div>
        <FormItemText
          name={"DigisellerID:"}
          onChange={handleChange("digiSellerIds")}
          value={item.digiSellerIds}
        />
        <div className={css.formItem}>
          <div className={css.name}>
            {!item.thirdPartyPriceType
              ? "Процент от Steam:"
              : "Цена Digiseller"}
          </div>
          <div
            className={classNames(css.doubleControl, "percent")}
            style={{ alignItems: "baseline" }}
          >
            <div className={css.wrapper}>
              <div className={css.inputControl}>
                <div className={css.inputArea}>
                  <TextBox
                    onChange={handleChange("thirdPartyPriceValue")}
                    defaultValue={item.thirdPartyPriceValue}
                    width={233}
                    type="number"
                  />
                  <Select
                    options={digiPriceSetType}
                    defaultValue={thirdPartyPriceTypeVal}
                    onChange={handleChange("thirdPartyPriceType")}
                    width={69}
                    height={75}
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
        {/* <div className={classNames(css.formItem, css.params)}>
          <div className={css.name}>
            {!item.thirdPartyPriceValue
              ? "Процент от Steam:"
              : "Цена Digiseller"}
          </div>
          <div style={{ width: "302px" }}>
            <div className={css.doubleControl} style={{ paddingLeft: "0px" }}>
              <TextBox
                onChange={handleChange("thirdPartyPriceValue")}
                defaultValue={item.thirdPartyPriceValue}
                width={"250px"}
                wrapperWidth={"revert-layer"}
                inputControlWidth={"115%"}
              />
              <Select
                options={digiPriceSetType}
                defaultValue={thirdPartyPriceTypeVal}
                onChange={handleChange("thirdPartyPriceType")}
                width={75}
                height={75}
              />
            </div>
          </div>
        </div> */}
      </div>
      <div className={css.actions}>
        <Button
          text={"Отобразить"}
          style={{
            backgroundColor: "#A348CE",
            marginRight: "24px",
            width: "322px",
          }}
          onClick={() => {
            handleOnSave(item);
          }}
        />
        <Button
          text={"Отмена"}
          onClick={async () => {
            if (onCancel) onCancel();
            //await setItem(initial);
          }}
          style={{ backgroundColor: "#9A7AA9", marginLeft: "0px" }}
        />
      </div>
    </ModalBase>
  );
};

export default ModalFilter;
