internal static void SedDiwydag(long _type, long _PanelPerfil, long _addModulo, long _cordenadX, long _cordenadY, long _cordenadZ, List < ModelRenderElement > _listRenderElement, string _ZRotate, long _dataWith)
{
    ModelRenderElement elementTdwidagFistLeven = new ModelRenderElement();
    elementTdwidagFistLeven.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/dywidag.stl";
    elementTdwidagFistLeven.CodeName = Atk60Element.GetDywidag(_dataWith + 240 + 150);
    elementTdwidagFistLeven.x = _cordenadX + 4;
    elementTdwidagFistLeven.y = _cordenadY - ((_dataWith / 10) / 2);
    elementTdwidagFistLeven.z = _cordenadZ + 215;
    elementTdwidagFistLeven.XRotate = 0;
    if (_type == 2) {
        elementTdwidagFistLeven.XRotate = 90;
        elementTdwidagFistLeven.x = _cordenadX - ((_dataWith / 10) / 2);
        elementTdwidagFistLeven.y = _cordenadY + 4;
    }
    _listRenderElement.Add(elementTdwidagFistLeven);
    ModelRenderElement elementTdwidagSecontLeven = new ModelRenderElement();
    elementTdwidagSecontLeven.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/dywidag.stl";
    elementTdwidagSecontLeven.CodeName = Atk60Element.GetDywidag(_dataWith + 240 + 150);
    elementTdwidagSecontLeven.x = _cordenadX + 4;
    elementTdwidagSecontLeven.y = _cordenadY - ((_dataWith / 10) / 2);
    elementTdwidagSecontLeven.z = _cordenadZ + 55;
    elementTdwidagSecontLeven.XRotate = 0;
    if (_type == 2) {
        elementTdwidagSecontLeven.XRotate = 90;
        elementTdwidagSecontLeven.x = _cordenadX - ((_dataWith / 10) / 2);
        elementTdwidagSecontLeven.y = _cordenadY + 4;
    }
    _listRenderElement.Add(elementTdwidagSecontLeven);

    ModelRenderElement elementPlacaFistLevel = new ModelRenderElement();
    elementPlacaFistLevel.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10443020.stl";
    elementPlacaFistLevel.CodeName = "10443020";
    elementPlacaFistLevel.x = _cordenadX + 4;
    elementPlacaFistLevel.y = _cordenadY + 13;
    elementPlacaFistLevel.z = _cordenadZ + 55;
    elementPlacaFistLevel.XRotate = 0;
    if (_type == 2) {
        elementPlacaFistLevel.XRotate = 90;
        elementPlacaFistLevel.x = _cordenadX + 13;
        elementPlacaFistLevel.y = _cordenadY + 4;
    }
    _listRenderElement.Add(elementPlacaFistLevel);

    ModelRenderElement elementPlacaSecontLevel = new ModelRenderElement();
    elementPlacaSecontLevel.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10443020.stl";
    elementPlacaSecontLevel.CodeName = "10443020";
    elementPlacaSecontLevel.x = _cordenadX + 4;
    elementPlacaSecontLevel.y = _cordenadY + 13;
    elementPlacaSecontLevel.z = _cordenadZ + 215;
    elementPlacaSecontLevel.XRotate = 0;
    if (_type == 2) {
        elementPlacaSecontLevel.XRotate = 90;
        elementPlacaSecontLevel.x = _cordenadX + 13;
        elementPlacaSecontLevel.y = _cordenadY + 4;
    }
    _listRenderElement.Add(elementPlacaSecontLevel);
    //Mirror
    ModelRenderElement elementTFistLevelMirror = new ModelRenderElement();
    elementTFistLevelMirror.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10443020.stl";
    elementTFistLevelMirror.CodeName = "10443020";
    elementTFistLevelMirror.x = _cordenadX + 4 + _addModulo;
    elementTFistLevelMirror.y = _cordenadY - _PanelPerfil - (_dataWith / 10) - 1;
    elementTFistLevelMirror.z = _cordenadZ + 55;
    elementTFistLevelMirror.XRotate = 180;
    elementTFistLevelMirror.ZRotate = _ZRotate;
    if (_type == 2) {
        elementTFistLevelMirror.XRotate = 270;
        elementTFistLevelMirror.x = _cordenadX - _PanelPerfil - (_dataWith / 10) - 1;
        elementTFistLevelMirror.y = _cordenadY + 4;
    }
    _listRenderElement.Add(elementTFistLevelMirror);

    ModelRenderElement elementTSecontLevelMirror = new ModelRenderElement();
    elementTSecontLevelMirror.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10443020.stl";
    elementTSecontLevelMirror.CodeName = "10443020";
    elementTSecontLevelMirror.x = _cordenadX + 4 + _addModulo;
    elementTSecontLevelMirror.y = _cordenadY - _PanelPerfil - (_dataWith / 10) - 1;
    elementTSecontLevelMirror.z = _cordenadZ + 215;
    elementTSecontLevelMirror.XRotate = 180;
    elementTSecontLevelMirror.ZRotate = _ZRotate;
    if (_type == 2) {
        elementTSecontLevelMirror.XRotate = 270;
        elementTSecontLevelMirror.x = _cordenadX - _PanelPerfil - (_dataWith / 10) - 1;
        elementTSecontLevelMirror.y = _cordenadY + 4;
    }
    _listRenderElement.Add(elementTSecontLevelMirror);

}