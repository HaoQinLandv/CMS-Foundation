﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.UI;
using Composite.C1Console.Forms;
using Composite.C1Console.Forms.CoreUiControls;
using Composite.C1Console.Forms.Plugins.UiControlFactory;
using Composite.C1Console.Forms.WebChannel;
using Composite.Plugins.Forms.WebChannel.Foundation;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ObjectBuilder;
using Microsoft.Practices.ObjectBuilder;


namespace Composite.Plugins.Forms.WebChannel.UiControlFactories
{
    /// <summary>    
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    public abstract class TypeSelectorTemplateUserControlBase : UserControl
    {
        private string _formControlLabel;
        private Type _selectedType;
        private TemplatedTypeSelectorUiControl _typeOptionsProxy;

        /// <exclude />
        protected abstract void BindStateToProperties();

        /// <exclude />
        protected abstract void InitializeViewState();

        /// <exclude />
        public abstract string GetDataFieldClientName();

        internal void BindStateToControlProperties()
        {
            this.BindStateToProperties();
        }

        internal void InitializeWebViewState()
        {
            this.InitializeViewState();
        }

        internal void SetTypeOptionsProxy(TemplatedTypeSelectorUiControl proxy)
        {
            _typeOptionsProxy = proxy;
        }

        /// <exclude />
        public Type SelectedType
        {
            get { return _selectedType; }
            set { _selectedType = value; }
        }

        /// <exclude />
        public string FormControlLabel
        {
            get { return _formControlLabel; }
            set { _formControlLabel = value; }
        }

        /// <exclude />
        public IEnumerable<Type> TypeOptions 
        {
            get
            {
                return _typeOptionsProxy.FetchTypeOptions();
            }
        }

    }




    internal sealed class TemplatedTypeSelectorUiControl : TypeSelectorUiControl, IWebUiControl
    {
        private Type _userControlType;
        private TypeSelectorTemplateUserControlBase _userControl;


        internal TemplatedTypeSelectorUiControl(Type userControlType)
        {
            _userControlType = userControlType;
        }


        /// <exclude />
        public override void BindStateToControlProperties()
        {
            _userControl.BindStateToControlProperties();
            this.SelectedType = _userControl.SelectedType;
        }


        /// <exclude />
        public void InitializeViewState()
        {
            _userControl.InitializeWebViewState();
        }


        /// <exclude />
        public Control BuildWebControl()
        {
            _userControl = _userControlType.ActivateAsUserControl<TypeSelectorTemplateUserControlBase>(this.UiControlID);

            _userControl.FormControlLabel = this.Label;
            _userControl.SelectedType = this.SelectedType;
            _userControl.SetTypeOptionsProxy(this);

            return _userControl;
        }


        internal IEnumerable<Type> FetchTypeOptions()
        {
            return base.GetTypeOptions();
        }


        /// <exclude />
        public bool IsFullWidthControl { get { return false; } }


        /// <exclude />
        public string ClientName { get { return _userControl.GetDataFieldClientName(); } }
    }



    [ConfigurationElementType(typeof(TemplatedTypeSelectorUiControlFactoryData))]
    internal sealed class TemplatedTypeSelectorUiControlFactory : Base.BaseTemplatedUiControlFactory
    {
        public TemplatedTypeSelectorUiControlFactory(TemplatedTypeSelectorUiControlFactoryData data)
            : base(data)
        { }

        public override IUiControl CreateControl()
        {
            TemplatedTypeSelectorUiControl control = new TemplatedTypeSelectorUiControl(this.UserControlType);

            return control;
        }
    }



    [Assembler(typeof(TemplatedTypeSelectorUiControlFactoryAssembler))]
    internal sealed class TemplatedTypeSelectorUiControlFactoryData : TypeSelectorUiControlFactoryData, Base.ITemplatedUiControlFactoryData
    {
        private const string _userControlVirtualPathPropertyName = "userControlVirtualPath";
        private const string _cacheCompiledUserControlTypePropertyName = "cacheCompiledUserControlType";

        [ConfigurationProperty(_userControlVirtualPathPropertyName, IsRequired = true)]
        public string UserControlVirtualPath
        {
            get { return (string)base[_userControlVirtualPathPropertyName]; }
            set { base[_userControlVirtualPathPropertyName] = value; }
        }

        [ConfigurationProperty(_cacheCompiledUserControlTypePropertyName, IsRequired = false, DefaultValue = true)]
        public bool CacheCompiledUserControlType
        {
            get { return (bool)base[_cacheCompiledUserControlTypePropertyName]; }
            set { base[_cacheCompiledUserControlTypePropertyName] = value; }
        }
    }



    internal sealed class TemplatedTypeSelectorUiControlFactoryAssembler : IAssembler<IUiControlFactory, UiControlFactoryData>
    {
        public IUiControlFactory Assemble(IBuilderContext context, UiControlFactoryData objectConfiguration, IConfigurationSource configurationSource, ConfigurationReflectionCache reflectionCache)
        {
            return new TemplatedTypeSelectorUiControlFactory(objectConfiguration as TemplatedTypeSelectorUiControlFactoryData);
        }
    }
}
