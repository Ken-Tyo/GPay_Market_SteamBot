import React from "react";
import css from "./styles.scss";

const SymbolTextBox = ({ hint, onChange, defaultValue, symbol, width }) => {
  const onChangeText = (event) => {
    let val = event.target.value;
    if (onChange) onChange(val);
  };

  return (
    <div className={css.wrapper} style={{ width: width }}>
      <div className={css.inputControl}>
        <div className={css.inputArea}>
          <input
            type={"text"}
            defaultValue={defaultValue}
            onChange={onChangeText}
          />
          {symbol && <div className={css.symbol}>{symbol}</div>}
        </div>
      </div>
      {hint && <div className={css.hint}>{hint}</div>}
    </div>
  );
};

export default SymbolTextBox;
