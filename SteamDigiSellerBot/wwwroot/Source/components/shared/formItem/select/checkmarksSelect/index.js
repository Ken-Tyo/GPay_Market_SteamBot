import * as React from 'react';
import OutlinedInput from '@mui/material/OutlinedInput';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import ListItemText from '@mui/material/ListItemText';
import Select from '@mui/material/Select';
import Checkbox from '@mui/material/Checkbox';

const ITEM_HEIGHT = 48;
const ITEM_PADDING_TOP = 8;
const MenuProps = {
  PaperProps: {
    style: {
      maxHeight: ITEM_HEIGHT * 4.5 + ITEM_PADDING_TOP,
      width: 250,
    },
  },
};



const MultipleSelectCheckmarks = ({options, onChange, value}) => {
  //const [personName, setPersonName] = React.useState([]);


  return (
    <div>
      <FormControl sx={{ m: 1, width: 300 }}>
        <InputLabel id="demo-multiple-checkbox-label">Tag</InputLabel>
        <Select
          labelId="demo-multiple-checkbox-label"
          id="demo-multiple-checkbox"
          multiple
          value={value}
          onChange={onChange}
          input={<OutlinedInput label="Tag" />}
          renderValue={(selected) => selected.map(e => e.name).join(", ")}
          MenuProps={MenuProps}
        >
          {options.map((obj) => (
            <MenuItem key={obj.id} value={obj}>
              <Checkbox checked={value.some(e => e.id == obj.id)} />
              <ListItemText primary={obj.name} />
            </MenuItem>
          ))}
        </Select>
      </FormControl>
    </div>
  );
}

export default MultipleSelectCheckmarks;