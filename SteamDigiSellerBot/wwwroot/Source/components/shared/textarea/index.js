import React from "react";
import css from "./styles.scss";

const Textarea = ({onChange, defaultValue, width, height, placeholder, value }) => {
  const onChangeTextarea = (event) => {
    let val = event.target.value;
    if (onChange) onChange(val);
  };

  return (
      <div className={css.wrapper} style={{ width: width }}>
        <div className={css.inputControl} style={{ height: height }}>
          <div className={css.boxes}>
            <textarea
                onChange={onChangeTextarea}
                rows="18"
                className={css.textarea}
                defaultValue={defaultValue}
                value={value}
                placeholder={placeholder}
                style={{ width: width, height: height }}
            ></textarea>
          </div>
        </div>
      </div>
  );
};

export default Textarea;
