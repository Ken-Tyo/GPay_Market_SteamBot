import SymbolTextBox from '../../SymbolTextbox';
import css from '../styles.scss';

const FormItemText = ({ name, onChange, hint, value, symbol }) => {
    return (
      <div className={css.formItem}>
        <div className={css.name}>{name}</div>
        <div>
          <SymbolTextBox
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