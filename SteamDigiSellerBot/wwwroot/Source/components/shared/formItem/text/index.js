import TextBox from "../../textbox2";
import css from "../styles.scss";

const FormItemText = ({ name, onChange, hint, value, symbol }) => {
  return (
    <div className={css.formItem}>
      <div className={css.name}>{name}</div>
      <div>
        <TextBox
          hint={hint}
          onChange={onChange}
          defaultValue={value}
          symbol={symbol}
        />
      </div>
    </div>
  );
};

export default FormItemText;
