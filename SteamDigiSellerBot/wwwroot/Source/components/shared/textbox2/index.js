import React from "react";
import css from "./styles.scss";

const TextBox = ({
  hint,
  onChange,
  defaultValue,
  cymbol,
  width = "302px",
  type = "text",
  wrapperWidth = "100%",
  inputControlWidth = "100%",
}) => {
  const onChangeText = (event) => {
    let val = event.target.value;
    if (onChange) onChange(val);
  };

  return (
    <div className={css.wrapper} style={{ width: wrapperWidth }}>
      <div className={css.inputControl} style={{ width: inputControlWidth }}>
        <div className={css.inputArea} style={{ width: width }}>
          <input
            type={type}
            value={defaultValue}
            onChange={onChangeText}
            //Защита от изменения прокрутки значения, есть type=numeric
            onFocus={(e) =>
              e.target.addEventListener("wheel", (e) => e.preventDefault(), {
                passive: false,
              })
            }
          />
          {cymbol && <div className={css.cymbol}>{cymbol}</div>}
        </div>
      </div>
      {hint && <div className={css.hint}>{hint}</div>}
    </div>
  );
};

export default TextBox;
