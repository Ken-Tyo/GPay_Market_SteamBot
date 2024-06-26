import React, { useEffect, useState } from "react";
import CircularProgress from "@mui/material/CircularProgress";
import css from "./styles.scss";

class InList extends React.Component {
  constructor(props) {
    super(props);
    this.state = { date: new Date() };
    this.callOnRender = this.callOnRender.bind(this);
  }
  callOnRender(value) {
    console.log("onRender " + value);
    if (this.props.onRender != null) {
      this.props.onRender(value);
    }
  }

  componentDidMount() {
    this.callOnRender(false);
  }

  componentWillUnmount() {}

  render() {
    return (
      <tbody>{this.props.data?.map((i) => this.props.itemRenderer(i))}</tbody>
    );
  }
}
const list = ({
  data,
  headers,
  itemRenderer,
  isLoading,
  loadingText,
  componentDidMount = null,
}) => {
  let headerIdx = 0;
  return (
    <div className={css.wrapper}>
      <div style={{ overflow: isLoading ? "hidden" : "inherit" }}>
        <table>
          <thead>
            <tr class="head">
              {headers?.map((h) => {
                let positionCss = "";
                if (headerIdx === 0) positionCss = css.first;
                else if (headerIdx === headers.length - 1)
                  positionCss = css.last;

                headerIdx++;
                return (
                  <th>
                    <div className={positionCss}>{h}</div>
                  </th>
                );
              })}
            </tr>
          </thead>
          {
            <InList
              data={data}
              onRender={componentDidMount}
              itemRenderer={itemRenderer}
            />
          }
        </table>
      </div>

      {isLoading && (
        <div className={css.dump}>
          <div className={css.loader}>
            <CircularProgress
              color="inherit"
              sx={{
                height: "99px !important",
                width: "99px !important",
              }}
            />
          </div>
          <div>
            {
              //Так сделано, чтобы была возможность при изменении внешних значений мог рассчитываться текст
              loadingText()
            }
          </div>
        </div>
      )}
    </div>
  );
};

export default list;
