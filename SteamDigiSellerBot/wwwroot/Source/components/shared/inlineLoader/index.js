import css from "./styles.scss";
import classNames from "classnames";
const InlineLoader = ({}) => {
  return (
    <div className={css.loaderContainer}>
      <div className={css.loaderBar}></div>
      <div
        className={classNames(css.loaderBar, css.smallBar, css.firstSmall)}
      ></div>
      <div
        className={classNames(css.loaderBar, css.smallBar, css.secondSmall)}
      ></div>
    </div>
  );
};

export default InlineLoader;
