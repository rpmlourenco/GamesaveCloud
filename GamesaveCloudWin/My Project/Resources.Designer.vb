﻿'------------------------------------------------------------------------------
' <auto-generated>
'     Este código foi gerado por uma ferramenta.
'     Versão de Tempo de Execução:4.0.30319.42000
'
'     As alterações a este ficheiro poderão provocar um comportamento incorrecto e perder-se-ão se
'     o código for regenerado.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'Essa classe foi gerada automaticamente pela classe StronglyTypedResourceBuilder
    'através de uma ferramenta como ResGen ou Visual Studio.
    'Para adicionar ou remover um associado, edite o arquivo .ResX e execute ResGen novamente
    'com a opção /str, ou recrie o projeto do VS.
    '''<summary>
    '''  Uma classe de recurso de tipo de alta segurança, para pesquisar cadeias de caracteres localizadas etc.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
     Global.Microsoft.VisualBasic.HideModuleNameAttribute()>  _
    Friend Module Resources
        
        Private resourceMan As Global.System.Resources.ResourceManager
        
        Private resourceCulture As Global.System.Globalization.CultureInfo
        
        '''<summary>
        '''  Retorna a instância de ResourceManager armazenada em cache usada por essa classe.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("GamesaveCloudWin.Resources", GetType(Resources).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Substitui a propriedade CurrentUICulture do thread atual para todas as
        '''  pesquisas de recursos que usam essa classe de recurso de tipo de alta segurança.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Consulta um recurso localizado do tipo System.Byte[].
        '''</summary>
        Friend ReadOnly Property drive_api() As Byte()
            Get
                Dim obj As Object = ResourceManager.GetObject("drive_api", resourceCulture)
                Return CType(obj,Byte())
            End Get
        End Property
        
        '''<summary>
        '''  Consulta um recurso localizado do tipo System.Drawing.Icon semelhante a (Ícone).
        '''</summary>
        Friend ReadOnly Property gamepad() As System.Drawing.Icon
            Get
                Dim obj As Object = ResourceManager.GetObject("gamepad", resourceCulture)
                Return CType(obj,System.Drawing.Icon)
            End Get
        End Property
        
        '''<summary>
        '''  Consulta um recurso localizado do tipo System.Byte[].
        '''</summary>
        Friend ReadOnly Property GamesaveDB() As Byte()
            Get
                Dim obj As Object = ResourceManager.GetObject("GamesaveDB", resourceCulture)
                Return CType(obj,Byte())
            End Get
        End Property
    End Module
End Namespace
