import React from "react";
import Select from "../../select";
import css from "../styles.scss";
import classNames from "classnames";

const FormItemSelect = ({
  name,
  onChange,
  value,
  options,
  hint,
  className,
}) => {
  return (
    <div className={classNames(css.formItem, className)}>
      <div className={css.name}>{name}</div>
      <div>
        <Select
          options={options}
          defaultValue={value}
          onChange={onChange}
          hint={hint}
        />
      </div>
    </div>
  );
};

export default FormItemSelect;
