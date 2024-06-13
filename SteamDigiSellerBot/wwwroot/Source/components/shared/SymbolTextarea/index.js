import React from "react";
import css from "./styles.scss";

const SymbolTextarea = ({onChange, defaultValue, width, placeholder }) => {
  const onChangeTextarea = (event) => {
    let val = event.target.value;
    if (onChange) onChange(val);
  };

  return (
    <div className={css.wrapper} style={{ width: width }}>
      <div className={css.inputControl}>
        <div className={css.boxes}>
          <textarea
            onChange={onChangeTextarea}
            rows="18"
            className={css.textarea}
            defaultValue={defaultValue}
            placeholder={placeholder}
          ></textarea>
        </div>
      </div>
    </div>
  );
};

export default SymbolTextarea;
