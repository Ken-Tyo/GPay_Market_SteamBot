import React from "react";
import css from "./styles.scss";

const TextBox = ({
  hint,
  onChange,
  defaultValue,
  cymbol,
  width,
  type = "text",
}) => {
  const onChangeText = (event) => {
    let val = event.target.value;
    if (onChange) onChange(val);
  };

  return (
    <div className={css.wrapper} style={{ width: "auto" }}>
      <div className={css.inputControl}>
        <div className={css.inputArea}>
          <input
            type={type}
            value={defaultValue}
            style={{ width: width }}
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
