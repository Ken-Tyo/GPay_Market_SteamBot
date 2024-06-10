import checkBoxCss from "./styles.scss";
import Checkbox from "@mui/material/Checkbox";

const FillCheckBox = ({ checked, size = 20, onChange }) => {
  const checkedSize = size - 8;
  const handleChange = (event, newValue) => {
    if (onChange) onChange(newValue);
  };
  return (
    <Checkbox
      icon={<div></div>}
      checkedIcon={
        <div
          style={{ width: `${checkedSize}px`, height: `${checkedSize}px` }}
          className={checkBoxCss.checked}
        ></div>
      }
      className={checkBoxCss.wrapper}
      sx={{ padding: "0px", width: `${size}px`, height: `${size}px` }}
      checked={checked}
      onChange={handleChange}
    />
  );
};

export default FillCheckBox;
