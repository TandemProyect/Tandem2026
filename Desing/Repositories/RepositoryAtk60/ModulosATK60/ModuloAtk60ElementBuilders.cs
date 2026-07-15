using Desing.Repositories.RepositoryCommun;
using System.Collections.Generic;

namespace Desing.Repositories.RepositoryAtk60.ModulosATK60
{
    public interface IModuloAtk60ElementBuilder
    {
        string ModuleCode { get; }
        int ModuleLengthMm { get; }
        long GetCount(ModulosAtk60Wall modulo);
    }

    internal abstract class ModuloAtk60ElementBuilderBase : IModuloAtk60ElementBuilder
    {
        public abstract string ModuleCode { get; }
        public abstract int ModuleLengthMm { get; }
        public abstract long GetCount(ModulosAtk60Wall modulo);
    }

    internal sealed class Modulo270Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M270";
        public override int ModuleLengthMm => 2700;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_270 : 0;
    }

    internal sealed class Modulo255Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M255";
        public override int ModuleLengthMm => 2550;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_255 : 0;
    }

    internal sealed class Modulo240Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M240";
        public override int ModuleLengthMm => 2400;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_240 : 0;
    }

    internal sealed class Modulo225Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M225";
        public override int ModuleLengthMm => 2250;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_225 : 0;
    }

    internal sealed class Modulo210Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M210";
        public override int ModuleLengthMm => 2100;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_210 : 0;
    }

    internal sealed class Modulo195Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M195";
        public override int ModuleLengthMm => 1950;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_195 : 0;
    }

    internal sealed class Modulo180Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M180";
        public override int ModuleLengthMm => 1800;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_180 : 0;
    }

    internal sealed class Modulo165Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M165";
        public override int ModuleLengthMm => 1650;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_165 : 0;
    }

    internal sealed class Modulo150Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M150";
        public override int ModuleLengthMm => 1500;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_150 : 0;
    }

    internal sealed class Modulo135Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M135";
        public override int ModuleLengthMm => 1350;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_135 : 0;
    }

    internal sealed class Modulo120Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M120";
        public override int ModuleLengthMm => 1200;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_120 : 0;
    }

    internal sealed class Modulo105Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M105";
        public override int ModuleLengthMm => 1050;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_105 : 0;
    }

    internal sealed class Modulo090Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M090";
        public override int ModuleLengthMm => 900;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_090 : 0;
    }

    internal sealed class Modulo075Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M075";
        public override int ModuleLengthMm => 750;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_075 : 0;
    }

    internal sealed class Modulo060Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M060";
        public override int ModuleLengthMm => 600;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_060 : 0;
    }

    internal sealed class Modulo045Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M045";
        public override int ModuleLengthMm => 450;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_045 : 0;
    }

    internal sealed class Modulo030Builder : ModuloAtk60ElementBuilderBase
    {
        public override string ModuleCode => "M030";
        public override int ModuleLengthMm => 300;
        public override long GetCount(ModulosAtk60Wall modulo) => modulo != null ? modulo.M_0430 : 0;
    }

    public static class ModuloAtk60ElementBuilderCatalog
    {
        public static List<IModuloAtk60ElementBuilder> CreateDefault()
        {
            return new List<IModuloAtk60ElementBuilder>
            {
                new Modulo270Builder(),
                new Modulo255Builder(),
                new Modulo240Builder(),
                new Modulo225Builder(),
                new Modulo210Builder(),
                new Modulo195Builder(),
                new Modulo180Builder(),
                new Modulo165Builder(),
                new Modulo150Builder(),
                new Modulo135Builder(),
                new Modulo120Builder(),
                new Modulo105Builder(),
                new Modulo090Builder(),
                new Modulo075Builder(),
                new Modulo060Builder(),
                new Modulo045Builder(),
                new Modulo030Builder(),
            };
        }
    }
}
