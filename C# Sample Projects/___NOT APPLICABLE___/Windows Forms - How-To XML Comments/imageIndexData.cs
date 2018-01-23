//*****************************************************************************
// Copyright (C) 1999-2002, Microsoft Corporation.  All Rights Reserved.
//*****************************************************************************

class ImageIndexData
{

    // This module stores datas for the imagestrip bitmap resource.  Indices
    // into the image strip are determined by (CS_IMG_xxx + CS_IMG_ACC_xxx)
    // where CS_IMG is the element group and CS_IMG_ACC is the accessibility
    // offset of that element.  For each element group the are six
    // accessibilities.

    public const int CS_IMG_IMGLIST_WIDTH  = 16   //Icon width.;
    public const int CS_IMG_NONE  = -1;
    public const int CS_IMG_UNKNOWN  = -1;

    //The accessibility offsets.

    public const int CS_IMG_ACC_PUBLIC = 0;
    public const int CS_IMG_ACC_INTERNAL = 1;
    public const int CS_IMG_ACC_FRIEND = 2;
    public const int CS_IMG_ACC_PROTECTED = 3;
    public const int CS_IMG_ACC_PROTECTEDFRIEND = 3;
    public const int CS_IMG_ACC_PRIVATE = 4;
    public const int CS_IMG_ACC_SHORTCUT = 5;
    public const int CS_IMG_ACC_TYPE_COUNT = 6   'The size of each element group.;

    //The element groups.

    public const int CS_IMG_CLASS  = (CS_IMG_ACC_TYPE_COUNT * 0);
    public const int CS_IMG_CONSTANT  = (CS_IMG_ACC_TYPE_COUNT * 1);
    public const int CS_IMG_DELEGATE  = (CS_IMG_ACC_TYPE_COUNT * 2);
    public const int CS_IMG_ENUM  = (CS_IMG_ACC_TYPE_COUNT * 3);
    public const int CS_IMG_ENUMMEMBER  = (CS_IMG_ACC_TYPE_COUNT * 4);
    public const int CS_IMG_EVENT  = (CS_IMG_ACC_TYPE_COUNT * 5);
    public const int CS_IMG_EXCEPTION  = (CS_IMG_ACC_TYPE_COUNT * 6);
    public const int CS_IMG_FIELD  = (CS_IMG_ACC_TYPE_COUNT * 7);
    public const int CS_IMG_INTERFACE  = (CS_IMG_ACC_TYPE_COUNT * 8);
    public const int CS_IMG_MACRO  = (CS_IMG_ACC_TYPE_COUNT * 9);
    public const int CS_IMG_MAP  = (CS_IMG_ACC_TYPE_COUNT * 10);
    public const int CS_IMG_MAPITEM  = (CS_IMG_ACC_TYPE_COUNT * 11);
    public const int CS_IMG_METHOD  = (CS_IMG_ACC_TYPE_COUNT * 12);
    public const int CS_IMG_OVERLOAD  = (CS_IMG_ACC_TYPE_COUNT * 13);
    public const int CS_IMG_MODULE  = (CS_IMG_ACC_TYPE_COUNT * 14);
    public const int CS_IMG_NAMESPACE  = (CS_IMG_ACC_TYPE_COUNT * 15);
    public const int CS_IMG_OPERATOR  = (CS_IMG_ACC_TYPE_COUNT * 16);
    public const int CS_IMG_PROPERTY  = (CS_IMG_ACC_TYPE_COUNT * 17);
    public const int CS_IMG_STRUCT  = (CS_IMG_ACC_TYPE_COUNT * 18);
    public const int CS_IMG_TEMPLATE  = (CS_IMG_ACC_TYPE_COUNT * 19);
    public const int CS_IMG_TYPEDEF  = (CS_IMG_ACC_TYPE_COUNT * 20);
    public const int CS_IMG_TYPE  = (CS_IMG_ACC_TYPE_COUNT * 21);
    public const int CS_IMG_UNION  = (CS_IMG_ACC_TYPE_COUNT * 22);
    public const int CS_IMG_VARIABLE  = (CS_IMG_ACC_TYPE_COUNT * 23);
    public const int CS_IMG_VALUETYPE  = (CS_IMG_ACC_TYPE_COUNT * 24);
    public const int CS_IMG_INTRINSIC  = (CS_IMG_ACC_TYPE_COUNT * 25);

    //The end of the image strip contains miscellaneous icons.  At this point,
    //the accessiblity offsets become meaningless.

    public const int CS_IMG_ERROR  = (CS_IMG_ACC_TYPE_COUNT * 26);
    public const int CS_IMG_ASSEMBLY  = (CS_IMG_ERROR + 6);
    public const int CS_IMG_LIBRARY  = (CS_IMG_ERROR + 7);
    public const int CS_IMG_CSPROJECT  = (CS_IMG_ERROR + 8);
    public const int CS_IMG_CSHARPPROJECT  = (CS_IMG_ERROR + 10);
    public const int CS_IMG_OPEN_FOLDER  = (CS_IMG_ERROR + 15);
    public const int CS_IMG_CLOSED_FOLDER  = (CS_IMG_ERROR + 16);
    public const int CS_IMG_ARROW  = (CS_IMG_ERROR + 17);
}

