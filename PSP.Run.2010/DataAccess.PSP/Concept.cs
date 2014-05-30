using System;
using System.Net;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Text;

namespace InfoBase {
  /// <summary>
  /// 与entities.py中定义的模型相对应 version:2009-4-18 14:17
  /// </summary>
  public class Concept {

    #region 属性

    public ConceptReference derivedFrom = new ConceptReference( string.Empty, string.Empty );

    public string name = string.Empty;
    public List<string> alsoKnownAs = new List<string>();
    public string description = string.Empty;
    //public Guid guid = Guid.NewGuid();
    public string guid = Guid.NewGuid().ToString();

    public List<ConceptReference> equivalentTo = new List<ConceptReference>();
    public List<ConceptReference> intersectionOf = new List<ConceptReference>();
    public List<ConceptReference> unionOf = new List<ConceptReference>();
    public List<ConceptReference> complementOf = new List<ConceptReference>();
    public List<ConceptReference> disjointWith = new List<ConceptReference>();

    public List<ConceptReference> belongsTo = new List<ConceptReference>();
    //public List<ConceptReference> contains = new List<ConceptReference>();
    public List<ConceptMember> contains = new List<ConceptMember>();

    public string valueRestriction = string.Empty;

    public string hownet = string.Empty;

    public List<string> tags = new List<string>();

    public DateTime created = DateTime.Now;
    public User createdBy = null;

    public DateTime modified = DateTime.Now;
    public User modifiedBy = null;

    public int versionNo = 0;

    public List<URI> seeAlso = new List<URI>();

    public string originalSource = string.Empty;

    #endregion 属性

    /// <summary>
    /// xml string to Concept list
    /// </summary>
    /// <param name="xml"></param>
    /// <returns></returns>
    public static List<Concept> XML2Concepts( string xml ) {
      if ( string.IsNullOrEmpty( xml ) )
        return null;
      List<Concept> cons = new List<Concept>();
      // XMLString --> Concept
      XElement xe = null;
      try {
        xe = XElement.Parse( xml );
      }
      catch ( Exception ex ) {
        return null;
      }
      if ( xe == null || xe.Element( "error") != null )
        return null;

      foreach ( XElement conceptElement in xe.Elements( "concept" ) ) {
        Concept c = XElement2Concept( conceptElement );
        if ( c != null )
          cons.Add( c );
      }

      return cons;
    }

    /// <summary>
    /// XElement to Concept
    /// </summary>
    /// <param name="conceptElement"></param>
    /// <returns></returns>
    public static Concept XElement2Concept( XElement conceptElement ) {
      if ( conceptElement == null )
        return null;
      Concept c = new Concept();
      XElement xeTmp = null;
      xeTmp = conceptElement.Element( "derivedFrom" );
      if ( xeTmp != null )
        c.derivedFrom = new ConceptReference( removeCommentTag( xeTmp.Attribute( "name" ).Value ), removeCommentTag( xeTmp.Attribute( "guid" ).Value ) );

      xeTmp = conceptElement.Element( "name" );
      if ( xeTmp != null )
        c.name = removeCommentTag( xeTmp.Value );
      xeTmp = conceptElement.Element( "alsoKnownAs" );
      if ( xeTmp != null && xeTmp.HasElements ) {
        foreach ( XElement xeAlsoKnownAs in xeTmp.Elements( "item" ) )
          c.alsoKnownAs.Add( removeCommentTag( xeAlsoKnownAs.Value ) );
      }
      xeTmp = conceptElement.Element( "description" );
      if ( xeTmp != null )
        c.description = removeCommentTag( xeTmp.Value );
      xeTmp = conceptElement.Element( "guid" );
      if ( xeTmp != null )
        //c.guid = new Guid( removeCommentTag( xeTmp.Value ) );
        c.guid = removeCommentTag( xeTmp.Value );

      xeTmp = conceptElement.Element( "equivalentTo" );
      if ( xeTmp != null && xeTmp.HasElements )
        foreach ( XElement xeItem in xeTmp.Elements( "item" ) ) {
          ConceptReference cr = new ConceptReference( removeCommentTag( xeItem.Attribute( "name" ).Value ), removeCommentTag( xeItem.Attribute( "guid" ).Value ) );
          c.equivalentTo.Add( cr );
        }
      xeTmp = conceptElement.Element( "intersetionOf" );
      if ( xeTmp != null && xeTmp.HasElements )
        foreach ( XElement xeItem in xeTmp.Elements( "item" ) ) {
          ConceptReference cr = new ConceptReference( removeCommentTag( xeItem.Attribute( "name" ).Value ), removeCommentTag( xeItem.Attribute( "guid" ).Value ) );
          c.intersectionOf.Add( cr );
        }
      xeTmp = conceptElement.Element( "unionOf" );
      if ( xeTmp != null && xeTmp.HasElements )
        foreach ( XElement xeItem in xeTmp.Elements( "item" ) ) {
          ConceptReference cr = new ConceptReference( removeCommentTag( xeItem.Attribute( "name" ).Value ), removeCommentTag( xeItem.Attribute( "guid" ).Value ) );
          c.unionOf.Add( cr );
        }
      xeTmp = conceptElement.Element( "complementOf" );
      if ( xeTmp != null && xeTmp.HasElements )
        foreach ( XElement xeItem in xeTmp.Elements( "item" ) ) {
          ConceptReference cr = new ConceptReference( removeCommentTag( xeItem.Attribute( "name" ).Value ), removeCommentTag( xeItem.Attribute( "guid" ).Value ) );
          c.complementOf.Add( cr );
        }
      xeTmp = conceptElement.Element( "disjointWith" );
      if ( xeTmp != null && xeTmp.HasElements )
        foreach ( XElement xeItem in xeTmp.Elements( "item" ) ) {
          ConceptReference cr = new ConceptReference( removeCommentTag( xeItem.Attribute( "name" ).Value ), removeCommentTag( xeItem.Attribute( "guid" ).Value ) );
          c.disjointWith.Add( cr );
        }

      xeTmp = conceptElement.Element( "belongsTo" );
      if ( xeTmp != null && xeTmp.HasElements )
        foreach ( XElement xeItem in xeTmp.Elements( "item" ) ) {
          ConceptReference cr = new ConceptReference( removeCommentTag( xeItem.Attribute( "name" ).Value ), removeCommentTag( xeItem.Attribute( "guid" ).Value ) );
          c.belongsTo.Add( cr );
        }
      xeTmp = conceptElement.Element( "contains" );
      if ( xeTmp != null && xeTmp.HasElements )
        foreach ( XElement xeItem in xeTmp.Elements( "item" ) ) {
          ConceptMember cm = new ConceptMember( xeItem.Attribute( "name" ).Value, xeItem.Attribute( "guid" ).Value );
          cm.minCardinality = xeItem.Attribute( "minCardinality" ).Value;
          cm.maxCardinality = xeItem.Attribute( "maxCardinality" ).Value;
          c.contains.Add( cm );
        }

      xeTmp = conceptElement.Element( "valueRestriction" );
      if ( xeTmp != null )
        c.valueRestriction = removeCommentTag( xeTmp.Value);

      xeTmp = conceptElement.Element( "hownet" );
      if ( xeTmp != null )
        c.hownet = removeCommentTag( xeTmp.Value );

      xeTmp = conceptElement.Element( "tags" );
      if ( xeTmp != null && xeTmp.HasElements ) {
        foreach ( XElement xeTag in xeTmp.Elements( "item" ) )
          c.tags.Add( removeCommentTag( xeTag.Value ) );
      }

      xeTmp = conceptElement.Element( "create" );
      if ( xeTmp != null ) {
        c.created = DateTime.Parse( removeCommentTag( xeTmp.Attribute( "create" ).Value ) );
        c.createdBy = new User( removeCommentTag( xeTmp.Attribute( "name" ).Value ), removeCommentTag( xeTmp.Attribute( "email" ).Value ) );
        c.createdBy.userType = removeCommentTag( xeTmp.Attribute( "userType" ).Value );
        c.createdBy.userId = removeCommentTag( xeTmp.Attribute( "userId" ).Value );
      }

      xeTmp = conceptElement.Element( "modify" );
      if ( xeTmp != null ) {
        c.modified = DateTime.Parse( removeCommentTag( xeTmp.Attribute( "modify" ).Value ) );
        c.modifiedBy = new User( removeCommentTag( xeTmp.Attribute( "name" ).Value ), removeCommentTag( xeTmp.Attribute( "email" ).Value ) );
        c.modifiedBy.userType = removeCommentTag( xeTmp.Attribute( "userType" ).Value );
        c.modifiedBy.userId = removeCommentTag( xeTmp.Attribute( "userId" ).Value );
      }

      xeTmp = conceptElement.Element( "versionNo" );
      if ( xeTmp != null )
        c.versionNo = Convert.ToInt32( removeCommentTag( xeTmp.Value ) );

      xeTmp = conceptElement.Element( "seeAlso" );
      if ( xeTmp != null && xeTmp.HasElements ) {
        foreach ( XElement xeSeeAlso in xeTmp.Elements( "item" ) ) {
          URI u = new URI( removeCommentTag( xeSeeAlso.Attribute( "title" ).Value ), removeCommentTag( xeSeeAlso.Attribute( "url" ).Value ) );
          c.seeAlso.Add( u );
        }
      }

      xeTmp = conceptElement.Element( "originalSource" );
      if ( xeTmp != null )
        c.originalSource = removeCommentTag( xeTmp.Value );

      return c;
    }

    /// <summary>
    /// xml string to Concept
    /// </summary>
    /// <param name="xmlConcept"></param>
    /// <returns></returns>
    public static Concept XMLString2Concept( string xmlConcept ) {
      if ( string.IsNullOrEmpty( xmlConcept ) )
        return null;
      XElement xe = null;
      try {
        xe = XElement.Parse( xmlConcept );
      }
      catch ( Exception ex ) {
        return null;
      }
      if ( xe == null || xe.Element( "error" ) != null )
        return null;
      return XElement2Concept( xe );
    }

    /// <summary>
    /// XElement to Concept--not complete
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static string Concept2XmlString( Concept c, bool withXMLHeader ) {
      if ( c == null )
        return String.Empty;

      XElement xroot = new XElement( "concept",
        new XElement( "name", new XCData( c.name ) ),
        new XElement( "guid", new XCData( c.guid.ToString() ) ),
        new XElement( "description", new XCData( c.description ) ),
        new XElement( "alsoKnownAs", from aka in c.alsoKnownAs select new XElement( "item", new XCData( aka ) ) ),
        new XElement( "derivedFrom", new XAttribute( "name", c.derivedFrom.name ), new XAttribute( "guid", c.derivedFrom.guid ) ),

        new XElement( "equivalentTo", from et in c.equivalentTo
                                      select new XElement( "item", new XAttribute( "name", et.name ), new XAttribute( "guid", et.guid ) ) ),
        new XElement( "intersectionOf", from et in c.intersectionOf
                                        select new XElement( "item", new XAttribute( "name", et.name ), new XAttribute( "guid", et.guid ) ) ),
        new XElement( "unionOf", from et in c.unionOf
                                 select new XElement( "item", new XAttribute( "name", et.name ), new XAttribute( "guid", et.guid ) ) ),
        new XElement( "complementOf", from et in c.complementOf
                                      select new XElement( "item", new XAttribute( "name", et.name ), new XAttribute( "guid", et.guid ) ) ),
        new XElement( "disjointWith", from et in c.disjointWith
                                      select new XElement( "item", new XAttribute( "name", et.name ), new XAttribute( "guid", et.guid ) ) ),

        new XElement( "belongsTo", from et in c.belongsTo
                                   select new XElement( "item", new XAttribute( "name", et.name ), new XAttribute( "guid", et.guid ) ) ),

        new XElement( "valueRestriction", new XCData( c.valueRestriction ) ),
        new XElement( "hownet", new XCData( c.hownet ) ),
        new XElement( "tags", from t in c.tags select new XElement( "item", new XCData( t ) ) ),
        new XElement( "seeAlso", from sa in c.seeAlso select new XElement( "item", new XAttribute( "title", sa.title ), new XAttribute( "url", sa.url ) ) ),
        new XElement( "originalSource", new XCData( c.originalSource ) ),
        new XElement( "versionNo", new XCData( c.versionNo.ToString() ) ),

        new XElement( "create", new XAttribute( "create", c.created.ToString( "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK" ) ),
          new XAttribute( "name", c.createdBy.name ), new XAttribute( "email", c.createdBy.email ), new XAttribute( "userType", c.createdBy.userType ), new XAttribute( "userId", c.createdBy.userId ) ),
        new XElement( "modify", new XAttribute( "modify", c.modified.ToString( "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK" ) ),
          new XAttribute( "name", c.modifiedBy.name ), new XAttribute( "email", c.modifiedBy.email ), new XAttribute( "userType", c.modifiedBy.userType ), new XAttribute( "userId", c.modifiedBy.userId ) )
      );
      // contains
      if ( c.contains != null )
        xroot.Add( new XElement( "contains", from et in c.contains
                                             select new XElement( "item", new XAttribute( "name", et.name ), new XAttribute( "guid", et.guid ),
                                               new XAttribute( "minCardinality", et.minCardinality ), new XAttribute( "maxCardinality", et.maxCardinality ) ) ) );

      return withXMLHeader ? "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + xroot.ToString() : xroot.ToString();
    }

    private static List<XElement> getXElements( List<string> values ) {
      List<XElement> xe = new List<XElement>();
      if ( values == null )
        return xe;
      foreach ( string str in values )
        xe.Add( new XElement( "item", new XCData( str ) ) );
      return xe;
    }
    private static List<XElement> getXElements_NameGuid( List<ConceptReference> values ) {
      List<XElement> xe = new List<XElement>();
      if ( values == null )
        return xe;
      foreach ( ConceptReference cr in values )
        xe.Add( new XElement( "item", new XAttribute( "name", cr.name ), new XAttribute( "guid", cr.guid ) ) );
      return xe;
    }
    private static List<XElement> getXElements_TitleUrl( List<URI> values ) {
      List<XElement> xe = new List<XElement>();
      if ( values == null )
        return xe;
      foreach ( URI u in values )
        xe.Add( new XElement( "item", new XAttribute( "title", u.title ), new XAttribute( "url", u.url ) ) );
      return xe;
    }

    private static string removeCommentTag( string str ) {
      // <![CDATA[Sun, 07 Dec 2008 00:03:00]]>
      return System.Text.RegularExpressions.Regex.Replace( str, @"(^<![CDATA[)|(]]>$)", "" ).Trim();
      //return System.Text.RegularExpressions.Regex.Replace( str, @"(<![CDATA[)|(&lt;![CDATA[)|(]]&gt;)|(]]>)", "" );
    }

    private static string addCommentTag( string str ) {
      // <![CDATA[Sun, 07 Dec 2008 00:03:00]]>
      return @"<![CDATA[" + str + "]]>";
    }

    /// <summary>
    /// 判断两个Concept除guid之外的内容是否相等
    /// </summary>
    /// <param name="ca"></param>
    /// <param name="cb"></param>
    /// <returns></returns>
    public static bool ConceptEquals( Concept ca, Concept cb ) {
      bool same = true;
      if ( ( ca == null && cb != null ) || ( ca != null && cb == null ) )
        return false;
      else if ( ca == null && cb == null )
        return true;

      if ( !string.Equals( ca.name, cb.name ) )
        return false;
      else if ( !string.Equals( ca.description, cb.description ) )
        return false;
      // contains, belongsTo
      else if ( !ConceptMembersEquals( ca.contains, cb.contains ) )
        return false;
      else if ( !ConceptReferencesEquals( ca.belongsTo, cb.belongsTo ) )
        return false;
      // equivalentTo, unionOf, complementOf, disjointWith, intersectionOf
      else if ( !ConceptReferencesEquals( ca.equivalentTo, cb.equivalentTo ) )
        return false;
      else if ( !ConceptReferencesEquals( ca.unionOf, cb.unionOf ) )
        return false;
      else if ( !ConceptReferencesEquals( ca.complementOf, cb.complementOf ) )
        return false;
      else if ( !ConceptReferencesEquals( ca.disjointWith, cb.disjointWith ) )
        return false;
      else if ( !ConceptReferencesEquals( ca.intersectionOf, cb.intersectionOf ) )
        return false;
      // seeAlso
      else if ( !URIsEquals( ca.seeAlso, cb.seeAlso ) )
        return false;
      // tags, alsoKnownAs
      else if ( !StringListEquals( ca.alsoKnownAs, cb.alsoKnownAs ) )
        return false;
      else if ( !StringListEquals( ca.tags, cb.tags ) )
        return false;
      // originalSource, hownet, valueRestriction
      else if ( !string.Equals( ca.originalSource, cb.originalSource ) )
        return false;
      else if ( !string.Equals( ca.hownet, cb.hownet ) )
        return false;
      else if ( !string.Equals( ca.valueRestriction, cb.valueRestriction ) )
        return false;
      else if ( !ConceptReferenceEquals( ca.derivedFrom, cb.derivedFrom ) )
        return false;

      return same;
    }

    static bool URIsEquals( List<URI> ulistA, List<URI> ulistB ) {
      if ( ulistA == null && ulistB == null )
        return true;
      else if ( ( ulistA == null && ulistB != null ) || ( ulistA != null && ulistB == null ) || ulistA.Count != ulistB.Count )
        return false;
      List<string> urlListA = new List<string>();
      foreach ( URI u in ulistA ) {
        urlListA.Add( u.url.Trim() );
      }
      foreach ( URI u in ulistB ) {
        if ( !urlListA.Contains( u.url.Trim() ) )
          return false;
      }
      return true;
    }

    static bool ConceptReferencesEquals( List<ConceptReference> ulistA, List<ConceptReference> ulistB ) {
      if ( ulistA == null && ulistB == null )
        return true;
      else if ( ( ulistA == null && ulistB != null ) || ( ulistA != null && ulistB == null ) || ulistA.Count != ulistB.Count )
        return false;
      List<string> urlListA = new List<string>();
      foreach ( ConceptReference u in ulistA ) {
        urlListA.Add( u.guid.Trim() );
      }
      foreach ( ConceptReference u in ulistB ) {
        if ( !urlListA.Contains( u.guid.Trim() ) )
          return false;
      }
      return true;
    }
    static bool ConceptReferenceEquals( ConceptReference crA, ConceptReference crtB ) {
      if ( crA == null && crtB == null )
        return true;
      else if ( ( crA == null && crtB != null ) || ( crA != null && crtB == null ) || !string.Equals( crA.guid, crtB.guid ) )
        return false;

      return true;
    }

    static bool ConceptMembersEquals( List<ConceptMember> ulistA, List<ConceptMember> ulistB ) {
      if ( ulistA == null && ulistB == null ) // 没用数据，表示相同
        return true;
      else if ( ( ulistA == null && ulistB != null ) || ( ulistA != null && ulistB == null ) || ulistA.Count != ulistB.Count ) // 条目个数不同，表示不同
        return false;
      else if ( ulistA.Count == ulistB.Count && ulistA.Count == 0 ) // 没用数据，表示相同
        return true;

      foreach ( ConceptMember ua in ulistA ) {
        bool sameGuid = false;
        foreach ( ConceptMember ub in ulistB ) {
          // 比较两个具有相同Guid的ConceptMember
          if ( ua.guid.Equals( ub.guid ) ) {
            sameGuid = true; // set tag
            bool eq = ConceptMemberEquals( ua, ub );
            if ( !eq )
              return false;
          }
        }
        // 有不一样的ConceptMember
        if ( !sameGuid )
          return false;
        // reset
        sameGuid = false;
      }

      return true;
    }
    static bool ConceptMemberEquals( ConceptMember crA, ConceptMember crtB ) {
      if ( crA == null && crtB == null )
        return true;
      else if ( ( crA == null && crtB != null ) || ( crA != null && crtB == null ) || !string.Equals( crA.guid, crtB.guid )
        || !string.Equals( crA.minCardinality, crtB.minCardinality ) || !string.Equals( crA.maxCardinality, crtB.maxCardinality ) )
        return false;

      return true;
    }

    static bool StringListEquals( List<string> strListA, List<string> strListB ) {
      if ( strListA == null && strListB == null )
        return true;
      else if ( ( strListA == null && strListB != null ) || ( strListA != null && strListB == null ) || strListA.Count != strListB.Count )
        return false;

      foreach ( string str in strListA ) {
        if ( !strListB.Contains( str ) )
          return false;
      }
      return true;
    }

  }

  /// <summary>
  /// (title, url)
  /// </summary>
  public class URI {
    public string title = string.Empty;
    public string url = string.Empty;

    public URI( string _title, string _url ) {
      title = _title;
      url = _url;
    }
  }

  /// <summary>
  /// 类似外键(name, guid)
  /// </summary>
  public class ConceptReference {
    public string name = string.Empty;
    public string guid = string.Empty;

    public ConceptReference( string _name, string _guid ) {
      name = _name;
      guid = _guid;
    }
  }

  /// <summary>
  /// 用于定义成员/属性，在ConceptReference的基础上加了基数约束
  /// </summary>
  public class ConceptMember {
    public string name { get; set; }
    public string guid { get; set; }
    public string minCardinality { get; set; }
    public string maxCardinality { get; set; }

    public ConceptMember( string _name, string _guid ) {
      name = _name;
      guid = _guid;
    }
  }

  /// <summary>
  /// User object
  /// </summary>
  public class User {
    public string name = string.Empty;
    public string email = string.Empty;
    public string userType = "google";
    public string userId = string.Empty;

    public User( string _name, string _email ) {
      name = _name;
      email = _email;
    }

    public User( string _name, string _email, string _userType ) {
      name = _name;
      email = _email;
      userType = _userType;
    }
  }

}
