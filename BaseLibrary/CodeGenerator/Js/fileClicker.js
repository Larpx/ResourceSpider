/// <reference path = "./base.page.ts" />
'use strict';
//文件选择自定义界面	<input type="file" id="YYY" /><div fileclicker="{FileId:'YYY'}" id="XXX"></div>
var AutoCSer;
(function (AutoCSer) {
    var FileClicker = (function () {
        function FileClicker(Parameter) {
            Pub.GetParameter(this, FileClicker.DefaultParameter, Parameter);
            this.Start(this.Event || DeclareEvent.Default);
        }
        FileClicker.prototype.Start = function (Event) {
            if (!Event.IsGetOnly) {
                var Element = HtmlElement.$Id(this.Id), Input = Element.Element0();
                if (Input != this.Element) {
                    this.Element = Input;
                    var FileInput = HtmlElement.$Id(this.FileId).Set('FILECLICKER', '{Id:"' + this.Id + '"}');
                    if (!this.IsFixed) {
                        Element.AddEvent('mousemove,mouseover,click', Pub.ThisEvent(this, this.Move));
                        FileInput.Style('outline', '0px').AddEvent('mousemove,mouseover', Pub.ThisEvent(this, this.Move));
                        this.SetCss();
                    }
                }
            }
        };
        FileClicker.prototype.SetCss = function () {
            HtmlElement.$Id(this.Id).Cursor('pointer');
            HtmlElement.$Id(this.FileId).Opacity(0).Style('position', 'absolute').Display(0).Set('size', 1).Cursor('pointer');
        };
        FileClicker.prototype.Move = function (Event) {
            HtmlElement.$Id(this.FileId).Style('left', (Event.clientX - 80) + 'px').Style('top', (Event.clientY - 8) + 'px').Display(1);
        };
        FileClicker.DefaultParameter = { Id: null, Event: null, FileId: null, IsFixed: false };
        return FileClicker;
    }());
    FileClicker = FileClicker;
    new Declare(FileClicker, 'FileClicker', 'mouseover', 'ParameterId');
})(AutoCSer || (AutoCSer = {}));
